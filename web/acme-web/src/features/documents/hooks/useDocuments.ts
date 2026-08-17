import { useQuery } from "@tanstack/react-query";

import { listDocuments } from "../api/listDocuments";

export function useDocuments() {
  return useQuery({
    queryKey: ["documents"],
    queryFn: listDocuments,
  });
}
