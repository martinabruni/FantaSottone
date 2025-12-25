import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { ButtonProps } from "@/components/ui/button";
import { forwardRef } from "react";

export type ActionType = "info" | "success" | "warning" | "error";

interface ActionButtonProps extends Omit<ButtonProps, "variant"> {
  actionType?: ActionType;
}

const actionVariants: Record<
  ActionType,
  { className: string; hoverClassName: string }
> = {
  info: {
    className: "bg-blue-600 text-white hover:bg-blue-700",
    hoverClassName: "hover:bg-blue-700",
  },
  success: {
    className: "bg-green-600 text-white hover:bg-green-700",
    hoverClassName: "hover:bg-green-700",
  },
  warning: {
    className: "bg-orange-600 text-white hover:bg-orange-700",
    hoverClassName: "hover:bg-orange-700",
  },
  error: {
    className: "bg-red-600 text-white hover:bg-red-700",
    hoverClassName: "hover:bg-red-700",
  },
};

export const ActionButton = forwardRef<HTMLButtonElement, ActionButtonProps>(
  ({ actionType = "info", className, children, ...props }, ref) => {
    const variantClass = actionVariants[actionType];

    return (
      <Button
        ref={ref}
        className={cn(variantClass.className, className)}
        {...props}
      >
        {children}
      </Button>
    );
  }
);

ActionButton.displayName = "ActionButton";
