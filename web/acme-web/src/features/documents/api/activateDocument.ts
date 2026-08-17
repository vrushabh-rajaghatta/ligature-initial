import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function activateDocument(documentId: string): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/documents/${documentId}/activate`),
    { method: "POST" },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Failed to activate Document."));
  }
}
