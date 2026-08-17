import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useDocuments } from "../hooks/useDocuments";
import { UploadDocumentDialog } from "../components/UploadDocumentDialog";
import { DocumentStatusBadge } from "../components/DocumentStatusBadge";

export function DocumentsListPage() {
  const navigate = useNavigate();

  const [dialogOpen, setDialogOpen] = useState(false);

  const { data, isLoading, error } = useDocuments();

  return (
    <Page>
      <PageHeader
        title="Documents"
        description="Manage your team's documents."
        actions={
          <Button onClick={() => setDialogOpen(true)}>Upload Document</Button>
        }
      />

      <UploadDocumentDialog open={dialogOpen} onOpenChange={setDialogOpen} />

      {isLoading && (
        <p className="text-muted-foreground">Loading Documents...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load Documents.</p>
      )}

      {!isLoading && !error && data?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <h3 className="text-lg font-semibold">
            No documents have been uploaded.
          </h3>

          <p className="mt-2 text-sm text-muted-foreground">
            Upload a file to create your first document.
          </p>

          <Button className="mt-4" onClick={() => setDialogOpen(true)}>
            Upload First Document
          </Button>
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="border-b bg-muted/40 text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2 font-medium">Name</th>
                <th className="px-4 py-2 font-medium">Status</th>
                <th className="px-4 py-2 font-medium">Current Version</th>
                <th className="px-4 py-2 font-medium">Created</th>
              </tr>
            </thead>

            <tbody>
              {data.map((document) => (
                <tr
                  key={document.id}
                  onClick={() => navigate(`/documents/${document.id}`)}
                  className="cursor-pointer border-b last:border-0 hover:bg-muted/40"
                >
                  <td className="px-4 py-2 font-medium">{document.name}</td>

                  <td className="px-4 py-2">
                    <DocumentStatusBadge status={document.status} />
                  </td>

                  <td className="px-4 py-2 text-muted-foreground">
                    v{document.currentVersionNumber}
                  </td>

                  <td className="px-4 py-2 text-muted-foreground">
                    {new Date(document.createdOnUtc).toLocaleDateString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Page>
  );
}
