import { GoogleLogin, CredentialResponse } from "@react-oauth/google";
import { useToast } from "@/hooks/useToast";

interface GoogleLoginButtonProps {
  onSuccess: (idToken: string) => Promise<void>;
  onError?: () => void;
}

export function GoogleLoginButton({
  onSuccess,
  onError,
}: GoogleLoginButtonProps) {
  const { toast } = useToast();

  const handleSuccess = async (credentialResponse: CredentialResponse) => {
    try {
      if (!credentialResponse.credential) {
        throw new Error("No credential received from Google");
      }

      await onSuccess(credentialResponse.credential);
    } catch (error) {
      console.error("Google login error:", error);
      toast({
        variant: "error",
        title: "Errore durante il login con Google",
        description:
          error instanceof Error
            ? error.message
            : "Si è verificato un errore imprevisto",
      });
      onError?.();
    }
  };

  const handleError = () => {
    console.error("Google login failed");
    toast({
      variant: "error",
      title: "Login con Google fallito",
      description: "Impossibile completare il login con Google",
    });
    onError?.();
  };

  return (
    <div className="flex justify-center">
      <GoogleLogin
        onSuccess={handleSuccess}
        onError={handleError}
        useOneTap
        theme="outline"
        size="large"
        text="signin_with"
        shape="rectangular"
      />
    </div>
  );
}
