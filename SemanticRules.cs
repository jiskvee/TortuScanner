public partial class Semantixer {
	void VisitarDIM(NodoDIM node) {
		var nombre = node.NombreVariable ?? "ERROR";
		var tipo = node.TipoDato?.ToUpper() ?? "ERROR";
		var exito = symtable.Declarar(nombre, tipo, node.Linea);
		if (!exito) ReportarError($"Variable '{nombre}' ya esta declarada", node.Linea);
	}

	void VisitarLET(NodoLET node) {
		var nombre = node.Variable ?? "";
		if (!symtable.YaDeclarado(nombre))
			ReportarError($"Variable '{nombre}' no fue declarada antes de asignacion", node.Linea);

		var tipoVar = symtable.TipoDe(nombre);
		var tipoExp = VisitarExpresion(node.Expresion);
		if (tipoExp is null) return;

		if (tipoVar == "INT" && tipoExp == "INT") {}
		else if (tipoVar == "REAL" && (tipoExp == "REAL" || tipoExp == "INT")) {}
		else
			ReportarError($"No se puede asignar expresion tipo '{tipoExp}' a la variable '{nombre}' tipo '{tipoVar}'", node.Linea);
	}

	string VisitarExpresion(NodoExp? exp) {
		switch (exp) {
			case NodoLiteral lit:
				if (lit.EsCadena)
					lit.TipoDato = "CADENA";

				var real = lit.Valor?.Contains('.') ?? false;
				lit.TipoDato = real ? "REAL" : "INT";
				return lit.TipoDato;

			case NodoVariableRef varRef:
				var nombre = varRef.Nombre ?? "ERROR";
				if (!symtable.YaDeclarado(nombre)) {
					ReportarError($"La variable '{nombre}' no ha sido declarada antes de usarse", varRef.Linea);
					return "ERROR";
				}
				varRef.TipoDato = symtable.TipoDe(nombre);
				return varRef.TipoDato;

			case NodoOperacion op:
				var izq = VisitarExpresion(op.Izquierda);
           		var der = VisitarExpresion(op.Derecha);

				if ((izq == "ERROR" || der == "ERROR") || (izq != "INT" && izq != "REAL") || (der != "INT" && der != "REAL")) {
                	ReportarError($"Operacion aritmetica '{op.Operador}' no valida entre tipos '{izq}' y '{der}'", op.Linea);
                	return "ERROR";
            	}

				op.TipoDato = (izq == "REAL" || der == "REAL") ? "REAL" : "INT";
				return op.TipoDato;

			default:
				return "ERROR";
		}
	}

	void VisitarINPUT(NodoINPUT node) {
		var nombre = node.Variable ?? "";
		if (!symtable.YaDeclarado(nombre))
			ReportarError($"Variable '{nombre}' no fue declarada antes del INPUT", node.Linea);
	}

	void VisitarPRINT(NodoPRINT nodo) {
		foreach (var elem in nodo.Elementos) {
			if (elem is NodoExp exp) VisitarExpresion(exp);
			else if (elem is not null) VisitarNodo(elem);
		}
	}

	void VisitarIF(NodoIF nodo) {
		if (nodo.Condicion != null) VisitarCondicion(nodo.Condicion);
		foreach (var inst in nodo.CuerpoVerdadero)
			VisitarNodo(inst);
		foreach (var inst in nodo.CuerpoFalso)
			VisitarNodo(inst);
	}

	private void VisitarCondicion(NodoCondicion cond) {
		var izq = VisitarExpresion(cond.Izquierda);
		var der = VisitarExpresion(cond.Derecha);

		bool ival = izq != "ERROR" || izq == "INT" || izq == "REAL";
		bool dval = der != "ERROR" || der == "INT" || der == "REAL";

		if (!ival || !dval)
			ReportarError($"La condicional '{cond.OperadorRelacional}' requiere ambos datos numericos (INT o REAL), se encontraron '{izq}' y '{der}'", cond.Linea);
	}

	private void VisitarWHILE(NodoWHILE nodo) {
		if (nodo.Condicion != null) VisitarCondicion(nodo.Condicion);
		foreach (var inst in nodo.Cuerpo)
			VisitarNodo(inst);
	}
}