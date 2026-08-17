import { NavLink } from "react-router-dom";

/**
 * One destination since ADR-066. This was role-aware while a platform
 * administrator managed tenants and a tenant administrator managed users;
 * a deployment now serves one customer, so only Users remains.
 */
export function PlatformSectionNavigation() {
  const items = [{ label: "Users", to: "/platform/users" }];

  return (
    <nav className="w-60 border-r p-3">
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          className="block rounded-md px-3 py-2 hover:bg-muted"
        >
          {item.label}
        </NavLink>
      ))}
    </nav>
  );
}
