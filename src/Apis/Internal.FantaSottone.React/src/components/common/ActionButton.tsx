import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { ButtonProps } from "@/components/ui/button";
import { forwardRef } from "react";

export type ActionType = "info" | "success" | "warning" | "error";

interface ActionButtonProps extends Omit<ButtonProps, "variant"> {
  actionType?: ActionType;
}

const actionVariants: Record<ActionType, string> = {
  info: "bg-blue-600 text-white hover:bg-blue-700 hover:shadow-lg hover:-translate-y-0.5 active:translate-y-0 active:shadow-md",
  success:
    "bg-green-600 text-white hover:bg-green-700 hover:shadow-lg hover:-translate-y-0.5 active:translate-y-0 active:shadow-md",
  warning:
    "bg-orange-600 text-white hover:bg-orange-700 hover:shadow-lg hover:-translate-y-0.5 active:translate-y-0 active:shadow-md",
  error:
    "bg-red-600 text-white hover:bg-red-700 hover:shadow-lg hover:-translate-y-0.5 active:translate-y-0 active:shadow-md",
};

export const ActionButton = forwardRef<HTMLButtonElement, ActionButtonProps>(
  ({ actionType = "info", className, children, ...props }, ref) => {
    const variantClass = actionVariants[actionType];

    return (
      <Button
        ref={ref}
        className={cn(
          "transition-all duration-200 ease-in-out",
          variantClass,
          className
        )}
        {...props}
      >
        {children}
      </Button>
    );
  }
);

ActionButton.displayName = "ActionButton";
