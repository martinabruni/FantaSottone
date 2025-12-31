import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
// import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { useToast } from "@/hooks/useToast";
import { ArrowLeft, Mail, Calendar } from "lucide-react";
import { UserProfileDto } from "@/types/user-types";
import { useUsers } from "@/providers/users/UsersProvider";

export function ProfilePage() {
  const [profile, setProfile] = useState<UserProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { toast } = useToast();
  const { getUserProfile } = useUsers();

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const response = await getUserProfile();
      setProfile(response.profile);
    } catch (error) {
      toast({
        variant: "error",
        title: "Errore nel caricamento",
        description: "Impossibile caricare il profilo",
      });
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("it-IT", {
      day: "2-digit",
      month: "long",
      year: "numeric",
    });
  };

  // const getInitials = (email: string) => {
  //   return email.substring(0, 2).toUpperCase();
  // };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">Caricamento...</div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">Profilo non trovato</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-2xl">
      <Button
        variant="ghost"
        onClick={() => navigate("/games")}
        className="mb-4"
      >
        <ArrowLeft className="mr-2 h-4 w-4" />
        Torna alle partite
      </Button>

      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">Profilo utente</CardTitle>
          <CardDescription>Le tue informazioni personali</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex items-center gap-4">
            {/* <Avatar className="h-20 w-20">
              <AvatarFallback className="text-2xl bg-primary text-primary-foreground">
                {getInitials(profile.email)}
              </AvatarFallback>
            </Avatar> */}
            <div>
              <h3 className="text-xl font-semibold">{profile.email}</h3>
              <p className="text-sm text-muted-foreground">
                ID Utente: {profile.userId}
              </p>
            </div>
          </div>

          <div className="space-y-4 pt-4 border-t">
            <div className="flex items-center gap-3">
              <div className="flex items-center justify-center h-10 w-10 rounded-full bg-muted">
                <Mail className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <p className="text-sm font-medium">Email</p>
                <p className="text-sm text-muted-foreground">{profile.email}</p>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <div className="flex items-center justify-center h-10 w-10 rounded-full bg-muted">
                <Calendar className="h-5 w-5 text-muted-foreground" />
              </div>
              <div>
                <p className="text-sm font-medium">Membro da</p>
                <p className="text-sm text-muted-foreground">
                  {formatDate(profile.createdAt)}
                </p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
