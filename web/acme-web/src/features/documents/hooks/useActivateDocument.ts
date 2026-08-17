import { useMutation, useQueryClient } from "@tanstack/react-query";

import { activateDocument } from "../api/activateDocument";

export function useActivateDocument(documentId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => activateDocument(documentId),

    onSuccess: () => {
      // ["documents"] is a prefix of every documents key, so this refreshes
      // both the detail (status badge + actions) and the list (status column).
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    },
  });
}
