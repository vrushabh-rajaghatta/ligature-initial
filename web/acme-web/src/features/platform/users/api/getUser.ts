import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { UserDetails } from "../types/UserDetails";

/** Distinguishes a genuine 404 from a transport/server failure. */
export class UserNotFoundError extends Error {}

export async function getUser(userId: string): Promise<UserDetails> {
  const response = await apiFetch(buildUrl(`/api/platform/users/${userId}`));

  if (response.status === 404) {
    throw new UserNotFoundError("User not found.");
  }

  if (!response.ok) {
    throw new Error("Unable to load user.");
  }

  return response.json();
}
