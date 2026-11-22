public class Cuadruplo {
	public string Op = "";
	public string Arg1 = "_";
	public string Arg2 = "_";
	public string Resultado = "_";
	public override string ToString() => $"({Op}, {Arg1}, {Arg2}, {Resultado})";
}

public partial class Cuadruplexer(SymbolTable table) {
	SymbolTable symtable = table;
	int tempcount = 0;
	int labelcount = 0;
	string NuevoTemp() => $"t{tempcount++}";
	string NuevaEtiqueta() => $"L{labelcount++}";

	public List<Cuadruplo> Cuadruplos = [];
	void Gen(string op, string arg1, string arg2, string resultado) {
		Cuadruplos.Add(new Cuadruplo{
			Op = op,
			Arg1 = arg1,
			Arg2 = arg2,
			Resultado = resultado
		});
	}

	public void Generar(NodoPrograma programa, bool imprimir = false) {
		foreach (var linea in programa.Instrucciones)
			VisitarNodo(linea);
		if (imprimir) {
			Console.WriteLine();
			G.Print("Cuadruplos generados exitosamente!! asombroso", ConsoleColor.Green);
			var i = 1;
			foreach (Cuadruplo cuad in Cuadruplos) {
				G.Print($"{i}. " + cuad.ToString(), ConsoleColor.White);
				i++;
			}
		}
	}

	public void VisitarNodo(AstNode node) {
		switch (node) {
			//case NodoDIM dim: GenerarDIM(dim); break;
			case NodoLET let: GenerarLET(let); break;
			case NodoINPUT inp: GenerarINPUT(inp); break;
			case NodoPRINT pri: GenerarPRINT(pri); break;
			case NodoIF iff: GenerarIF(iff); break;
			case NodoWHILE whi: GenerarWHILE(whi); break;
			case NodoCondicion con: GenerarCondicion(con); break;
			case NodoExp exp: GenerarExpresion(exp); break;
		}
	}
}