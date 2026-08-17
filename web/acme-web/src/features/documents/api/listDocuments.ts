import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { DocumentSummary } from "../types/DocumentSummary";

export async function listDocuments(): Promise<DocumentSummary[]> {
  const response = await apiFetch(buildUrl("/api/documents"));

  if (!response.ok) {
    throw new Error("Failed to load Documents.");
  }

  return response.json();
}
