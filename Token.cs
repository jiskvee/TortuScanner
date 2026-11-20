public enum TokenType {
	Identificador = 100,
	NumEntero = 101,
	NumReal = 102,
	MenorQue = 103,
	MenorIgual = 104,
	DiferenteDe = 105,
	MayorQue = 106,
	MayorIgual = 107,
	Cadena = 108,
	Coma = 109,
	Mas = 110,
	Menos = 111,
	Por = 112,
	Entre = 113,
	Igual = 114,
	AbrirParentesis = 115,
	CerrarParentesis = 116,

	Salto,
	Reservada,
	Comentario,
	EOF,

	Error_CaracterInvalido = 500,
	Error_NumeroMalformado = 501,
	Error_CadenaSinCerrar = 502,
	Error_Apocalipsis,
}

public class Token(TokenType tipo, string lexema, int linea) {
    public TokenType Tipo = tipo;
    public string Lexema = lexema;
    public int Linea = linea;
	public bool Is(string palabra) => Lexema.Equals(palabra, G.ic);
	public override string ToString() => $"> {Tipo}: '{Lexema}' (linea {Linea})";

	public static bool operator ==(Token? token, TokenType type) => token is not null && token.Tipo == type;
	public static bool operator !=(Token? token, TokenType type) => !(token == type);

	public static bool operator ==(TokenType type, Token? token) => token == type;
    public static bool operator !=(TokenType type, Token? token) => !(token == type);

	public override bool Equals(object? obj) {
        if (obj is Token other) return Tipo == other.Tipo && Lexema == other.Lexema && Linea == other.Linea;
        if (obj is TokenType type) return Tipo == type;
        return false;
    }
    public override int GetHashCode() => HashCode.Combine(Tipo, Lexema, Linea);
}
