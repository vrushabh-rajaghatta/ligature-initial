import { PlatformNavigationItem } from "./PlatformNavigationItem";

// `to` links a module to its landing route.
const modules: { title: string; to?: string }[] = [
  { title: "Documents", to: "/documents" },
  { title: "Platform", to: "/platform/users" },
  { title: "Settings", to: "/settings/security" },
];

export function PlatformNavigation() {
  return (
    <aside className="w-64 border-r p-3">
      {modules.map((module) => (
        <PlatformNavigationItem
          key={module.title}
          title={module.title}
          to={module.to}
        />
      ))}
    </aside>
  );
}
