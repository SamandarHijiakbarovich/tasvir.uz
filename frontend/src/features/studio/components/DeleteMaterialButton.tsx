"use client";

import { deleteMaterialAction } from "../actions";

export function DeleteMaterialButton({ id, title }: { id: string; title: string }) {
  return (
    <form
      action={deleteMaterialAction.bind(null, id)}
      onSubmit={(event) => {
        if (!window.confirm(`“${title}” o‘chirilsinmi? Buni qaytarib bo‘lmaydi.`)) event.preventDefault();
      }}
    >
      <button className="tv-btn tv-btn-text tv-btn-danger" type="submit">
        O‘chirish
      </button>
    </form>
  );
}
