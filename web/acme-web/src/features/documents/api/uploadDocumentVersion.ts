import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface UploadDocumentVersionResponse {
  versionId: string;
  versionNumber: number;
}

export async function uploadDocumentVersion(
  documentId: string,
  file: File,
): Promise<UploadDocumentVersionResponse> {
  const form = new FormData();
  form.append("file", file);

  // No Content-Type header — the browser sets the multipart boundary.
  const response = await apiFetch(
    buildUrl(`/api/documents/${documentId}/versions`),
    {
      method: "POST",
      body: form,
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Failed to upload new version."));
  }

  return response.json();
}
