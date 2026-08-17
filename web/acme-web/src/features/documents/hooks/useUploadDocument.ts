import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  uploadDocument,
  type UploadDocumentRequest,
} from "../api/uploadDocument";

export function useUploadDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UploadDocumentRequest) => uploadDocument(request),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    },
  });
}
