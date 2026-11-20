/**
cada registro es para un solo simbolo, guardara su propio indice en la tabla,
y un hash con todas las lineas en donde el simbolo esta presente, bastante shrimple
**/
public class SymbolRegistry
{
	public int Indice;
	public string Lexema = "";
	public string Tipo = "ERROR";
	public bool Declarado = false;

	// hash para los numeros de linea!
	// asi no repetimos si el id esta dos veces en una linea
	// de nuevo, como solo importa el valor y no la llave, hash es asombroso
	public HashSet<int> Lineas = [];

	// convierte el set hash a un string de forma "1, 2, 3..."
	public string GetLinesString() {
		bool first = true;
		string lines = "";
		foreach (var item in Lineas) {
			if (first) {
				lines += item;
				first = false;
			} else
				lines += $", {item}";
		} return lines;
	}

	public override string ToString() => $"{Indice} | {Lexema} | {Tipo} | {GetLinesString()}";
}


public class SymbolTable
{
	// el diccionario para los identificadores
	// como no se deben repetir un diccionario es lo mas sencillo
	public readonly Dictionary<string, SymbolRegistry> Dict = new(StringComparer.OrdinalIgnoreCase);
	int Next = 1;

	// crea un nuevo registro de simbolo en la tabla
	// devuelve si tuvo exito, entonces si es falso ese simbolo ya existia
	public bool Declarar(string lexema, string tipo, int linea) {
		if (Existe(lexema, out SymbolRegistry? registro) && registro is not null) {
			if (registro.Declarado) return false;
			registro.Tipo = tipo;
			registro.Declarado = true;
			registro.Lineas.Add(linea);
			return true;
		}

		var reg = new SymbolRegistry { Indice = Next++, Tipo = tipo, Lexema = lexema, Declarado = true };
		reg.Lineas.Add(linea);
		Dict.Add(lexema, reg);
		return true;
	}

	// añade una linea a un registro de simbolo existente
	public void AñadirLinea(string lexema, int linea) {
		if (Existe(lexema, out SymbolRegistry? registro) && registro is not null) {
			registro.Lineas.Add(linea);
			return;
		}

		var reg = new SymbolRegistry { Indice = Next++, Tipo = "ERROR", Lexema = lexema, Declarado = false };
		reg.Lineas.Add(linea);
		Dict.Add(lexema, reg);
	}

	// checa si un simbolo existe
	public bool Existe(string lexema) => Dict.TryGetValue(lexema, out _);
	public bool Existe(string lexema, out SymbolRegistry? registro) {
		var response = Dict.TryGetValue(lexema, out SymbolRegistry? reg);
		registro = reg;
		return response;
	}

	// checa si un simbolo esta DECLARADO, diferente de existir
	public bool YaDeclarado(string lexema) {
		Dict.TryGetValue(lexema, out SymbolRegistry? reg);
		if (reg is null) return false;
		return reg.Declarado;
	}

	// obtiene el registro de un simbolo
	public SymbolRegistry? Get(string lexema) {
		Dict.TryGetValue(lexema, out SymbolRegistry? reg);
		return reg;
	}

	// obtiene el tipo del dato un simbolo
	public string TipoDe(string lexema) => Get(lexema)?.Tipo.ToUpper() ?? "ERROR";

	// busca si hay un registro del identificador para añadirle una linea
	// si no existe, crea un nuevo registro
	// ahora mismo no se usa en nada
	public int Put(string lexema, string tipo, int linea) {
		if (Existe(lexema, out SymbolRegistry? registro)) {
			registro?.Lineas.Add(linea);
			return registro?.Indice ?? 0;
		}

		var reg = new SymbolRegistry { Indice = Next++, Tipo = tipo, Lexema = lexema };
		reg.Lineas.Add(linea);
		Dict.Add(lexema, reg);
		return reg.Indice;
	}

	// imprime la tabla de forma bien bonita
	public void Print() {
		string format = "{0,-10} {1,-15} {2,-15} {3,-15}";
		object[] header = ["Indice", "Lexema", "Tipo", "Lineas"];
		Console.WriteLine(format, header);
		Console.WriteLine(new string('-', 55));

		foreach (var item in Dict.Values) {
			object[] body = [item.Indice, item.Lexema, item.Tipo, item.GetLinesString()];
			Console.WriteLine(format, body);
		}

		Console.WriteLine(new string('-', 55));
	}
}