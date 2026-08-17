export type UserRole = "Administrator" | "Member";

export interface CurrentUser {
  userId: string;
  email: string;
  role: UserRole;
}
