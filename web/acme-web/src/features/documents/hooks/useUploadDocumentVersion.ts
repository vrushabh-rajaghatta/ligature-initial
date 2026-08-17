import { useMutation, useQueryClient } from "@tanstack/react-query";

import { uploadDocumentVersion } from "../api/uploadDocumentVersion";

export function useUploadDocumentVersion(documentId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => uploadDocumentVersion(documentId, file),

    onSuccess: () => {
      // ["documents"] is a prefix of every documents key, so this refreshes
      // both the detail (version table) and the list (current version column).
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    },
  });
}
