public partial class Cuadruplexer {
	void GenerarLET(NodoLET node) {
		var dir = GenerarExpresion(node.Expresion);
		Gen("=", dir, "_", node.Variable);
	}

	string GenerarExpresion(NodoExp exp) {
		switch (exp) {
			case NodoLiteral lit:
				return lit.Valor;
			case NodoVariableRef varRef:
				return varRef.Nombre;
			case NodoOperacion op:
				var izq = GenerarExpresion(op.Izquierda);
				var der = GenerarExpresion(op.Derecha);
				var temp = NuevoTemp();
				Gen(op.Operador, izq, der, temp);
				return temp;
			default:
				return "0";
		}
	}

	void GenerarINPUT(NodoINPUT node) {
		Gen("READ", "_", "_", node.Variable);
	}
	void GenerarPRINT(NodoPRINT node) {
		foreach (var elem in node.Elementos) {
			if (elem is NodoLiteral lit)
				Gen("WRITE", lit.Valor, "_", "_");
			else if (elem is NodoExp exp) {
				var dir = GenerarExpresion(exp);
				Gen("WRITE", dir, "_", "_");
			}
		}
	}

	void GenerarIF(NodoIF node) {
		var cond = GenerarCondicion(node.Condicion);

		if (node.CuerpoFalso.Count == 0) { // IF sin ELSE
			var fin = NuevaEtiqueta();
			Gen("JZ", cond, "_", fin);
			foreach (var inst in node.CuerpoVerdadero) VisitarNodo(inst);
			Gen("LABEL", "_", "_", fin);

		} else { // IF con ELSE
			var sino = NuevaEtiqueta();
			var fin = NuevaEtiqueta();

			Gen("JZ", cond, "_", sino);
			foreach (var inst in node.CuerpoVerdadero) VisitarNodo(inst);
			Gen("JMP", "_", "_", fin);
			Gen("LABEL", "_", "_", sino);
			foreach (var inst in node.CuerpoFalso) VisitarNodo(inst);
			Gen("LABEL", "_", "_", fin);
		}
	}

	string GenerarCondicion(NodoCondicion cond) {
		var izq = GenerarExpresion(cond.Izquierda);
		var der = GenerarExpresion(cond.Derecha);
		var temp = NuevoTemp();
		Gen(cond.OperadorRelacional, izq, der, temp);
		return temp;
	}

	void GenerarWHILE(NodoWHILE node) {
		var inicio = NuevaEtiqueta();
		var fin = NuevaEtiqueta();

		Gen("LABEL", "_", "_", inicio);
		var cond = GenerarCondicion(node.Condicion);
		Gen("JZ", cond, "_", fin);
		foreach (var inst in node.Cuerpo) VisitarNodo(inst);
		Gen("JMP", "_", "_", inicio);
		Gen("LABEL", "_", "_", fin);
	}
}