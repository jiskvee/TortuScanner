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
	public string? NombreVariable;
	public string? TipoDato;
}

public class NodoLET : AstNode {
	public string? Variable;
	public NodoExp? Expresion;
}

public class NodoINPUT : AstNode {
	public string? Variable;
}

public class NodoPRINT : AstNode {
	public List<AstNode> Elementos = [];
}

public class NodoIF : AstNode {
	public NodoCondicion? Condicion;
	public List<AstNode> CuerpoVerdadero = [];
	public List<AstNode> CuerpoFalso = [];
}

public class NodoWHILE : AstNode {
	public NodoCondicion? Condicion;
	public List<AstNode> Cuerpo = [];
}

// expresiones
public abstract class NodoExp : AstNode {
	public string? TipoDato;
}

public class NodoOperacion : NodoExp {
	public NodoExp? Izquierda;
	public string? Operador;
	public NodoExp? Derecha;
}

public class NodoLiteral : NodoExp {
	public string? Valor;
	public bool EsCadena;
}

public class NodoVariableRef : NodoExp {
	public string? Nombre;
}

// condiciones
public class NodoCondicion : AstNode {
	public NodoExp? Izquierda;
	public string? OperadorRelacional;
	public NodoExp? Derecha;
}