import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { DocumentDetail } from "../types/DocumentDetail";

export async function getDocument(documentId: string): Promise<DocumentDetail> {
  const response = await apiFetch(buildUrl(`/api/documents/${documentId}`));

  if (response.status === 404) {
    throw new Error("Document not found.");
  }

  if (!response.ok) {
    throw new Error("Unable to load Document.");
  }

  return response.json();
}
