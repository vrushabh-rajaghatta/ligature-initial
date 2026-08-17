import { useNavigate } from "react-router-dom";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { useUploadDocument } from "../hooks/useUploadDocument";
import {
  uploadDocumentSchema,
  type UploadDocumentFormValues,
} from "../validation/uploadDocumentSchema";

interface Props {
  onSuccess: () => void;
}

export function UploadDocumentForm({ onSuccess }: Props) {
  const navigate = useNavigate();

  const mutation = useUploadDocument();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<UploadDocumentFormValues>({
    resolver: zodResolver(uploadDocumentSchema),
    defaultValues: {
      name: "",
    },
  });

  async function onSubmit(values: UploadDocumentFormValues) {
    const { id } = await mutation.mutateAsync({
      name: values.name,
      file: values.file,
    });

    reset();
    onSuccess();

    // Uploading is how a document is created — take the user straight into
    // the new document's detail page.
    navigate(`/documents/${id}`);
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="name">Document Name</FieldLabel>

              <Input
                id="name"
                placeholder="e.g. Employee Handbook 2026"
                {...field}
              />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="file"
          render={({ field }) => (
            <Field data-invalid={!!errors.file}>
              <FieldLabel htmlFor="file">File</FieldLabel>

              <input
                id="file"
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
          {mutation.isPending ? "Uploading..." : "Upload Document"}
        </Button>
      </div>
    </form>
  );
}
