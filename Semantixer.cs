public partial class Semantixer(SymbolTable table) {
	SymbolTable symtable = table;
	public List<string> Errores = [];

	void ReportarError(string msg, int linea) => Errores.Add($"Error semantico en linea {linea}: {msg}");

	public void Analizar(NodoPrograma root) {
		foreach(var ins in root.Instrucciones)
			VisitarNodo(ins);

		Console.WriteLine();
		if (Errores.Count > 0) {
			Console.WriteLine("=== ERRORES SEMANTICOS REPORTADOS ===");
			foreach (var err in Errores)
				G.Print(err, ConsoleColor.Red);
		} else
			G.Print("Analisis semantico completado sin errores!! poggers", ConsoleColor.Green);

	}

	public void VisitarNodo(AstNode node) {
		switch (node) {
			case NodoDIM dim: VisitarDIM(dim); break;
			case NodoLET let: VisitarLET(let); break;
			case NodoINPUT inp: VisitarINPUT(inp); break;
			case NodoPRINT pri: VisitarPRINT(pri); break;
			case NodoIF iff: VisitarIF(iff); break;
			case NodoWHILE whi: VisitarWHILE(whi); break;
			case NodoCondicion con: VisitarCondicion(con); break;
		}
	}
}