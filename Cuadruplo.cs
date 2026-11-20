public class Cuadruplo {
	public string Op = "";
	public string Arg1 = "_";
	public string Arg2 = "_";
	public string Resultado = "_";
	public override string ToString() => $"({Op}, {Arg1}, {Arg2}, {Resultado})";
}

public class CuadruploInador {
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

	public void Generar(NodoLineas programa) {
		foreach (var linea in programa.Lineas) {
			// hacer cosas bien fregonas como recorrer el arbol
			//VisitarNodo(linea);
			// y asi
		}
	}
}