// Toast styling utilities to ensure backgrounds are not transparent

export const toastVariantClasses = {
  default: "bg-background border-border",
  success: "bg-green-600 text-white border-green-700",
  error: "bg-red-600 text-white border-red-700",
  warning: "bg-orange-600 text-white border-orange-700",
  info: "bg-blue-600 text-white border-blue-700",
  destructive: "bg-red-600 text-white border-red-700",
};

export function getToastClass(
  variant?: keyof typeof toastVariantClasses
): string {
  return toastVariantClasses[variant || "default"];
}
