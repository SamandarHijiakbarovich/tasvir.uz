"use client";

import { deleteMaterialAction } from "../actions";

export function DeleteMaterialButton({ id, title }: { id: string; title: string }) {
  return (
    <form
      action={deleteMaterialAction.bind(null, id)}
      onSubmit={(event) => {
        if (!window.confirm(`“${title}” öçirilsinmi? Buni qaytarib bölmaydi.`)) event.preventDefault();
      }}
    >
      <button className="tv-btn tv-btn-text tv-btn-danger" type="submit">
        Öçiriş
      </button>
    </form>
  );
}
