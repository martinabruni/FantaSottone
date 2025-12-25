import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { ButtonProps } from "@/components/ui/button";
import { forwardRef } from "react";

export type ActionType = "info" | "success" | "warning" | "error";

interface ActionButtonProps extends Omit<ButtonProps, "variant"> {
  actionType?: ActionType;
}

const actionVariants: Record<ActionType, ButtonProps["variant"]> = {
  info: "info",
  success: "success",
  warning: "warning",
  error: "error",
};

export const ActionButton = forwardRef<HTMLButtonElement, ActionButtonProps>(
  ({ actionType = "info", className, children, ...props }, ref) => {
    const variant = actionVariants[actionType];

    return (
      <Button
        ref={ref}
        variant={variant}
        className={cn(className)}
        {...props}
      >
        {children}
      </Button>
    );
  }
);

ActionButton.displayName = "ActionButton";
