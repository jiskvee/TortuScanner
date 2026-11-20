public abstract class AstNode {
	public int Linea;
}

public class NodoPrograma : AstNode {
	public List<AstNode> Instrucciones = [];
}
public class NodoLineas : AstNode {
	public List<AstNode> Lineas = [];
}

// instrucciones
public class NodoDIM : AstNode {
	public required string NombreVariable;
	public string TipoDato = "INT";
}

public class NodoLET : AstNode {
	public required string Variable;
	public required NodoExp Expresion;
}

public class NodoINPUT : AstNode {
	public required string Variable;
}

public class NodoPRINT : AstNode {
	public List<AstNode> Elementos = [];
}

public class NodoIF : AstNode {
	public required NodoCondicion Condicion;
	public List<AstNode> CuerpoVerdadero = [];
	public List<AstNode> CuerpoFalso = [];
}

public class NodoWHILE : AstNode {
	public required NodoCondicion Condicion;
	public List<AstNode> Cuerpo = [];
}

// expresiones
public abstract class NodoExp : AstNode {
	public string TipoDato = "INT";
}

public class NodoOperacion : NodoExp {
	public required NodoExp Izquierda;
	public required string Operador;
	public required NodoExp Derecha;
}

public class NodoLiteral : NodoExp {
	public required string Valor;
	public bool EsCadena;
}

public class NodoVariableRef : NodoExp {
	public required string Nombre;
}

// condiciones
public class NodoCondicion : AstNode {
	public required NodoExp Izquierda;
	public required string OperadorRelacional;
	public required NodoExp Derecha;
}