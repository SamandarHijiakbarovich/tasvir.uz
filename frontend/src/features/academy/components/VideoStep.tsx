import type { VideoContent } from "../api/types";

// ID seed’dan keladi, lekin iframe manziliga tushishidan oldin baribir tekshiriladi
const VALID_ID = /^[\w-]{11}$/;

export function VideoStep({ content }: { content: VideoContent }) {
  const isValid = VALID_ID.test(content.youtubeId);
  const watchUrl = `https://www.youtube.com/watch?v=${encodeURIComponent(content.youtubeId)}`;

  return (
    <div className="tv-step-body">
      <p>{content.intro}</p>
      {isValid && (
        <div className="tv-video">
          {/* youtube-nocookie: foydalanuvchi play bosmaguncha YouTube kuzatuv cookie’lari yozilmaydi */}
          <iframe
            src={`https://www.youtube-nocookie.com/embed/${content.youtubeId}?rel=0`}
            title={content.sourceTitle}
            loading="lazy"
            referrerPolicy="strict-origin-when-cross-origin"
            allow="accelerometer; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
          />
        </div>
      )}
      <p className="tv-video-source">
        Video: “{content.sourceTitle}”. Muallifning YouTube’dagi darsi.{" "}
        <a href={watchUrl} target="_blank" rel="noopener noreferrer">
          YouTube’da ochish ↗
        </a>
      </p>
    </div>
  );
}
