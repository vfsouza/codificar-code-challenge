namespace Codificar.Web;

public static class Formatacao {
	public static string DataHora(this DateTime utc) => utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

	public static string DataHora(this DateTime? utc) => utc?.DataHora() ?? "—";
}
