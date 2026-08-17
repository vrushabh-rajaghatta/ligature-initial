import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface UploadDocumentRequest {
  name: string;
  file: File;
}

export interface UploadDocumentResponse {
  id: string;
}

export async function uploadDocument(
  request: UploadDocumentRequest,
): Promise<UploadDocumentResponse> {
  const form = new FormData();
  form.append("file", request.file);
  form.append("name", request.name);

  // No Content-Type header — the browser sets the multipart boundary.
  const response = await apiFetch(buildUrl("/api/documents"), {
    method: "POST",
    body: form,
  });

  if (!response.ok) {
    throw new Error(await detailOf(response, "Failed to upload Document."));
  }

  return response.json();
}
