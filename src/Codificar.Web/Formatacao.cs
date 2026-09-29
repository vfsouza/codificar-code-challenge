namespace Codificar.Web;

public static class Formatacao {
	/// <summary>Converte a data (gravada em UTC) para o fuso local e formata para exibição.</summary>
	public static string DataHora(this DateTime utc) => utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

	public static string DataHora(this DateTime? utc) => utc?.DataHora() ?? "—";
}
