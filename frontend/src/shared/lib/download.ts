/** Faylni brauzerda saqlash. Faqat klient kodida ishlatiladi. */
export function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

/** SVG ni aniqroq PNG ga aylantirib yuklab beradi. */
export function downloadSvgAsPng(svg: string, width: number, height: number, fileName: string, scale = 2): Promise<void> {
  return new Promise((resolve, reject) => {
    const image = new Image();
    image.onload = () => {
      const canvas = document.createElement("canvas");
      canvas.width = width * scale;
      canvas.height = height * scale;
      canvas.getContext("2d")!.drawImage(image, 0, 0, canvas.width, canvas.height);
      canvas.toBlob((blob) => {
        if (!blob) return reject(new Error("PNG yaratilmadi"));
        downloadBlob(blob, fileName);
        resolve();
      }, "image/png");
    };
    image.onerror = () => reject(new Error("Rasm yuklanmadi"));
    image.src = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
  });
}

export function toSafeFileName(name: string) {
  const cleaned = name.trim().replace(/[\\/:*?"<>|]+/g, "").replace(/\s+/g, "-").slice(0, 60);
  return cleaned || "material";
}
