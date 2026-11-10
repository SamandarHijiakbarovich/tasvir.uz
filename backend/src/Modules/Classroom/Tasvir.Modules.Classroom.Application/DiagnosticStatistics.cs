namespace Tasvir.Modules.Classroom.Application;

/// <summary>
/// Bir guruhda boshlang‘ich va yakuniy natija farqi (juftlangan Styudent mezoni).
/// T va P o‘sishlar bir xil bo‘lsa (dispersiya nol), hisoblanmaydi.
/// EffectSize: Koen dz = o‘rtacha o‘sish / o‘sishlar standart chetlanishi.
/// </summary>
public sealed record PairedTestDto(int N, double MeanPre, double MeanPost, double MeanGain, double SdGain, double? T, int Df, double? P, double? EffectSize);

/// <summary>Ikki guruh o‘rtachalari farqi (Uelch varianti, dispersiyalar teng bo‘lishi shart emas). EffectSize: Koen d.</summary>
public sealed record TwoSampleTestDto(int CountA, double MeanA, double SdA, int CountB, double MeanB, double SdB, double? T, double? Df, double? P, double? EffectSize);

public static class DiagnosticStatistics
{
    public const double Alpha = 0.05;

    public static PairedTestDto? Paired(IReadOnlyList<(int Pre, int Post)> pairs)
    {
        var n = pairs.Count;
        if (n < 2)
        {
            return null;
        }

        var gains = pairs.Select(p => (double)(p.Post - p.Pre)).ToList();
        var meanGain = gains.Average();
        var sd = StandardDeviation(gains);
        double? t = sd > 0 ? meanGain / (sd / Math.Sqrt(n)) : null;

        return new PairedTestDto(
            n,
            pairs.Average(p => p.Pre),
            pairs.Average(p => p.Post),
            meanGain,
            sd,
            t,
            n - 1,
            t is { } value ? TwoTailedP(value, n - 1) : null,
            sd > 0 ? meanGain / sd : null);
    }

    public static TwoSampleTestDto? Welch(IReadOnlyList<int> a, IReadOnlyList<int> b)
    {
        if (a.Count < 2 || b.Count < 2)
        {
            return null;
        }

        var xa = a.Select(v => (double)v).ToList();
        var xb = b.Select(v => (double)v).ToList();
        double ma = xa.Average(), mb = xb.Average();
        double sa = StandardDeviation(xa), sb = StandardDeviation(xb);
        double va = sa * sa / a.Count, vb = sb * sb / b.Count;
        var se = Math.Sqrt(va + vb);

        double? t = null, df = null, p = null, d = null;
        if (se > 0)
        {
            t = (ma - mb) / se;
            df = (va + vb) * (va + vb) / (va * va / (a.Count - 1) + vb * vb / (b.Count - 1));
            p = TwoTailedP(t.Value, df.Value);
        }

        var pooled = Math.Sqrt(((a.Count - 1) * sa * sa + (b.Count - 1) * sb * sb) / (a.Count + b.Count - 2));
        if (pooled > 0)
        {
            d = (ma - mb) / pooled;
        }

        return new TwoSampleTestDto(a.Count, ma, sa, b.Count, mb, sb, t, df, p, d);
    }

    /// <summary>Styudent taqsimotida ikki tomonlama p-qiymat: P(|T| ≥ |t|) = I_{df/(df+t²)}(df/2, 1/2).</summary>
    public static double TwoTailedP(double t, double df) => RegularizedBeta(df / 2, 0.5, df / (df + t * t));

    private static double StandardDeviation(List<double> values)
    {
        var mean = values.Average();
        return Math.Sqrt(values.Sum(v => (v - mean) * (v - mean)) / (values.Count - 1));
    }

    // Quyidagi uchta funksiya "Numerical Recipes" dagi standart algoritmlar (betai, betacf, gammln)

    private static double RegularizedBeta(double a, double b, double x)
    {
        if (x <= 0)
        {
            return 0;
        }

        if (x >= 1)
        {
            return 1;
        }

        var front = Math.Exp(LogGamma(a + b) - LogGamma(a) - LogGamma(b) + a * Math.Log(x) + b * Math.Log(1 - x));
        return x < (a + 1) / (a + b + 2)
            ? front * BetaContinuedFraction(a, b, x) / a
            : 1 - front * BetaContinuedFraction(b, a, 1 - x) / b;
    }

    private static double BetaContinuedFraction(double a, double b, double x)
    {
        const int maxIterations = 300;
        const double epsilon = 1e-14;
        const double tiny = 1e-300;

        double qab = a + b, qap = a + 1, qam = a - 1;
        var c = 1.0;
        var d = 1 - qab * x / qap;
        if (Math.Abs(d) < tiny) d = tiny;
        d = 1 / d;
        var h = d;

        for (var m = 1; m <= maxIterations; m++)
        {
            var m2 = 2 * m;
            var aa = m * (b - m) * x / ((qam + m2) * (a + m2));
            d = 1 + aa * d;
            if (Math.Abs(d) < tiny) d = tiny;
            c = 1 + aa / c;
            if (Math.Abs(c) < tiny) c = tiny;
            d = 1 / d;
            h *= d * c;

            aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2));
            d = 1 + aa * d;
            if (Math.Abs(d) < tiny) d = tiny;
            c = 1 + aa / c;
            if (Math.Abs(c) < tiny) c = tiny;
            d = 1 / d;
            var delta = d * c;
            h *= delta;
            if (Math.Abs(delta - 1) < epsilon)
            {
                break;
            }
        }

        return h;
    }

    private static double LogGamma(double x)
    {
        double[] coefficients = [76.18009172947146, -86.50532032941677, 24.01409824083091, -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5];
        var y = x;
        var tmp = x + 5.5;
        tmp -= (x + 0.5) * Math.Log(tmp);
        var series = 1.000000000190015;
        foreach (var c in coefficients)
        {
            series += c / ++y;
        }

        return -tmp + Math.Log(2.5066282746310005 * series / x);
    }
}
