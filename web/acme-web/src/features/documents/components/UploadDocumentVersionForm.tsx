import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";

import { useUploadDocumentVersion } from "../hooks/useUploadDocumentVersion";
import {
  uploadDocumentVersionSchema,
  type UploadDocumentVersionFormValues,
} from "../validation/uploadDocumentVersionSchema";

interface Props {
  documentId: string;
  onSuccess: () => void;
}

export function UploadDocumentVersionForm({ documentId, onSuccess }: Props) {
  const mutation = useUploadDocumentVersion(documentId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<UploadDocumentVersionFormValues>({
    resolver: zodResolver(uploadDocumentVersionSchema),
  });

  async function onSubmit(values: UploadDocumentVersionFormValues) {
    await mutation.mutateAsync(values.file);

    reset();
    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="file"
          render={({ field }) => (
            <Field data-invalid={!!errors.file}>
              <FieldLabel htmlFor="version-file">File</FieldLabel>

              <input
                id="version-file"
                type="file"
                onChange={(event) =>
                  field.onChange(event.target.files?.[0] ?? undefined)
                }
                className="block w-full text-sm text-muted-foreground file:mr-4 file:rounded-md file:border-0 file:bg-primary file:px-3 file:py-2 file:text-sm file:font-medium file:text-primary-foreground hover:file:bg-primary/90"
              />

              <FieldError errors={[errors.file]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm font-normal text-destructive">
          {mutation.error.message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Uploading..." : "Upload Version"}
        </Button>
      </div>
    </form>
  );
}
