namespace Farmhollow
{
    // Spieler-Rang (kommt vom Server / aus der users-Tabelle, Spalte "role").
    public enum UserRole { Player, Moderator, Admin }

    public static class Rank
    {
        // Farben (Rich-Text-Hex) fuer die Chat-Tags
        public const string AdminColor  = "#FF3B30"; // rot
        public const string ModColor    = "#3B82F6"; // blau
        public const string PlayerColor = "#E6C84F"; // gelblich

        public static UserRole Parse(string role)
        {
            if (role == "admin") return UserRole.Admin;
            if (role == "moderator") return UserRole.Moderator;
            return UserRole.Player;
        }

        // Prefix vor dem Namen im Chat, z. B. "<color=#FF3B30>[ADMINISTRATOR]</color> "
        public static string ChatPrefix(UserRole r)
        {
            switch (r)
            {
                case UserRole.Admin:     return "<color=" + AdminColor + ">[ADMINISTRATOR]</color> ";
                case UserRole.Moderator: return "<color=" + ModColor + ">[MODERATOR]</color> ";
                default:                 return "<color=" + PlayerColor + ">[Spieler]</color> ";
            }
        }

        // Komplette Chat-Zeile: [Rang] Name: Nachricht
        public static string FormatChatLine(UserRole r, string name, string message)
        {
            return ChatPrefix(r) + "<b>" + name + "</b>: " + message;
        }
    }
}
