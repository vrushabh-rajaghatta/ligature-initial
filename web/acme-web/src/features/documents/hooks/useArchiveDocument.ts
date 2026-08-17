import { useMutation, useQueryClient } from "@tanstack/react-query";

import { archiveDocument } from "../api/archiveDocument";

export function useArchiveDocument(documentId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => archiveDocument(documentId),

    onSuccess: () => {
      // ["documents"] is a prefix of every documents key, so this refreshes
      // both the detail (status badge + actions) and the list (status column).
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    },
  });
}
