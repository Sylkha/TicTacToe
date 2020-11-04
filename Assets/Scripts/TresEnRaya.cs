using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TresEnRaya : MonoBehaviour
{
    /// <summary>
    /// Las fichas
    /// </summary>
    [SerializeField] GameObject cruz;
    [SerializeField] GameObject circulo;

    [SerializeField] Text turno_texto;

    /// <summary>
    /// Estas son las casillas.
    /// </summary>
    [SerializeField] GameObject[] casillas = new GameObject[9];

    public enum Turno { cruz, circulo, vacio};
    /// <summary>
    /// El seguimiento del tablero actual
    /// </summary>
    Turno turno;
    Turno[] board = new Turno[9];

    public enum algoritmo { MINIMAX, NEGAMAX, NEGAMAXPRUN, ASPIRATION_SEARCH, NEGASCOUT };
    public algoritmo alg;

    // El score es el resultado que nos darán los algoritmos como valor. El movimiento será la casilla correspondiente a ese valor.
    public struct ScoreMov
    {
        public int score;
        public int move;
    }
    ScoreMov scoreMov;

    [SerializeField] int max_depth = 50;

    void Awake()
    {
        turno = Turno.cruz;
        turno_texto.text = "Turno del Jugador 1";

        // Desde el principio van a estar vacíos y luego los iremos rellenando según vaya avanzando el juego.
        for (int i = 0; i < 9; i++)
            board[i] = Turno.vacio;
    }

    public void SeleccionarCasilla(byte ID, GameObject casilla)
    {
        if(turno == Turno.cruz)
        {
            // Hacemos que aparezca una cruz en la casilla marcada y guardamos quién ha puesto ahí su ficha
            casillas[ID] = Instantiate(cruz, casilla.transform.position, Quaternion.identity);
            board[ID] = turno;

            // Tras colocar la ficha, comprobamos si con ese movimiento ha ganado o no.
            // Si ha ganado, se terminó el juego y notificamos quién ha ganado
            // Si aún no ha ganado, pasamos al siguiente turno.
            if (CondicionVictoria(turno, board))
            {
                turno = Turno.vacio;    // Para que el jugador no siga dandole a más casillas.
                turno_texto.text = "¡Ha ganado el Jugador 1!";
            }
            else
            {
                turno = Turno.circulo;
                turno_texto.text = "Turno del Jugador 2";
            }
        }
        #region minimax
        if (turno == Turno.circulo && alg == algoritmo.MINIMAX)
        {
            int mejor_valor = -1;
            int mejor_casilla = -1;
            int valor;

            // Hacemos minimax para encontrar el mejor valor que tengamos y nos lo guardamos. Nos interesa la posición que elegimos como primer movimiento
            for(int i = 0; i < 9; i++)
            {
                if(board[i] == Turno.vacio)
                {
                    board[i] = Turno.circulo;
                    valor = minimax(Turno.cruz, board, 1, -1000, +1000);
                    board[i] = Turno.vacio;

                    if(mejor_valor < valor)
                    {
                        mejor_valor = valor;
                        mejor_casilla = i;
                    }
                }
            }

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if(mejor_casilla > -1)
            {
                casillas[mejor_casilla] = Instantiate(circulo, casillas[mejor_casilla].transform.position, Quaternion.identity);
                board[mejor_casilla] = turno;
            }

            // La misma condición de victoria
            if (CondicionVictoria(turno, board))
            {
                turno = Turno.vacio;    // Para que el jugador no siga dandole a más casillas.
                turno_texto.text = "¡Ha ganado el Jugador 2!";
            }
            else
            {
                turno = Turno.cruz;
                turno_texto.text = "Turno del Jugador 1";
            }
        }
        #endregion minimax

        #region negamax
        if (turno == Turno.circulo && alg == algoritmo.NEGAMAX) // aqui solo recojo la mejor casilla y valor
        {
            scoreMov = negamax(board, 1);

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos] = Instantiate(circulo, casillas[bestPos].transform.position, Quaternion.identity);
                board[bestPos] = turno;
            }

            // La misma condición de victoria
            if (CondicionVictoria(turno, board))
            {
                turno = Turno.vacio;    // Para que el jugador no siga dandole a más casillas.
                turno_texto.text = "¡Ha ganado el Jugador 2!";
            }
            else
            {
                turno = Turno.cruz;
                turno_texto.text = "Turno del Jugador 1";
            }
        }
        #endregion negamax

        #region negamaxpruning
        if (turno == Turno.circulo && alg == algoritmo.NEGAMAXPRUN)
        {
            scoreMov = negamaxAB(board, 1, -1000, 1000);

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos] = Instantiate(circulo, casillas[bestPos].transform.position, Quaternion.identity);
                board[bestPos] = turno;
            }

            // La misma condición de victoria
            if (CondicionVictoria(turno, board))
            {
                turno = Turno.vacio;    // Para que el jugador no siga dandole a más casillas.
                turno_texto.text = "¡Ha ganado el Jugador 2!";
            }
            else
            {
                turno = Turno.cruz;
                turno_texto.text = "Turno del Jugador 1";
            }
        }
        #endregion negamaxpruning

        #region busquedaAspitacional
        if (turno == Turno.circulo && alg == algoritmo.ASPIRATION_SEARCH)
        {
            scoreMov = AspirationSearch(board);

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos] = Instantiate(circulo, casillas[bestPos].transform.position, Quaternion.identity);
                board[bestPos] = turno;
            }

            // La misma condición de victoria
            if (CondicionVictoria(turno, board))
            {
                turno = Turno.vacio;    // Para que el jugador no siga dandole a más casillas.
                turno_texto.text = "¡Ha ganado el Jugador 2!";
            }
            else
            {
                turno = Turno.cruz;
                turno_texto.text = "Turno del Jugador 1";
            }
        }
        #endregion busquedaAspitacional

        #region negascout
        if (turno == Turno.circulo && alg == algoritmo.NEGASCOUT)
        {
            scoreMov = negascout(board, 0, -1000, 1000);

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos] = Instantiate(circulo, casillas[bestPos].transform.position, Quaternion.identity);
                board[bestPos] = turno;
            }

            // La misma condición de victoria
            if (CondicionVictoria(turno, board))
            {
                turno = Turno.vacio;    // Para que el jugador no siga dandole a más casillas.
                turno_texto.text = "¡Ha ganado el Jugador 2!";
            }
            else
            {
                turno = Turno.cruz;
                turno_texto.text = "Turno del Jugador 1";
            }
        }
        #endregion negascout
        ////////////////////////////////////////////////////////////////
        // Si hay empate
        if (Empate(board))
        {
            turno = Turno.vacio;
            turno_texto.text = "Empate";
        }

        // Desactivamos la casilla para que no le podamos volver a dar.
        casilla.SetActive(false);
    }

    #region minimaxFunc
    int minimax(Turno _turno, Turno[] _board, int depth, int alpha, int beta)
    {
        // Este juego es de suma cero, ya que, mientras uno gana (+1) el otro pierde (-1), o nadie gana (0).
        // Para cada vuelta que demos en esta función, iremos comprobando si el movimiento que prueba la IA le da la victoria, pierde, o termina en empate
        if (CondicionVictoria(Turno.circulo, _board))
            return +100;
        else if (CondicionVictoria(Turno.cruz, _board))
            return -100;
        else if (Empate(_board))
            return 0;
        if(depth == max_depth)
        {
            Debug.Log("evaluacion!!");
            return otherEvaluation(_turno, _board);
        }
        int valor;

        if(_turno == Turno.circulo)
        {
            // Vamos comprobando qué resultado lógico saldría de cada opción que tenemos en el tablero (las casillas vacías).
            for(int i = 0; i < 9; i++)
            {
                if(_board[i] == Turno.vacio)
                {
                    _board[i] = Turno.circulo;
                    valor = minimax(Turno.cruz, _board, depth + 1, alpha, beta); // sacamos la respuesta posible del jugador
                    _board[i] = Turno.vacio; // Volvemos a dejarlo como estaba

                    // Como viene en la lógica del minimax, si el valor que estamos estudiando en max, es mayor que el valor que tengamos en alpha,
                    // nos quedamos con ese valor (es la mejor opción para nosotros como IA), si no, seguimos buscando.
                    if (valor > alpha)
                        alpha = valor;

                    // Alpha beta pruning. Si alpha es mayor que beta, no habrá ningún valor más interesante que el que ya tenemos, así que dejamos de buscar.
                    if (alpha > beta) 
                        break;
                }
            }
            return alpha;
        }

        // Ponemos else directamente porque nunca meteremos si es el turno de "vacio", ya que ese es nuestro estado para no seguir ejecutando código
        else 
        {
            for(int i = 0; i < 9; i++)
            {
                if(_board[i] == Turno.vacio)
                {
                    _board[i] = Turno.cruz;
                    valor = minimax(Turno.circulo, _board, depth + 1, alpha, beta);
                    _board[i] = Turno.vacio;

                    // Como estamos estudiando la parte del min, si encontramos un valor menor que el que tenemos (beta), nos lo quedamos.
                    if (valor < beta)
                        beta = valor;

                    // Alpha beta pruning. Si alpha es mayor que beta, no habrá ningún valor más interesante que el que ya tenemos, así que dejamos de buscar.
                    if (alpha > beta)
                        break;

                }
            }

            return beta;
        }

    }
    #endregion minimaxFunc
    
    #region negamaxFunc
    ScoreMov negamax(Turno[] board, int depth)
    {
        ScoreMov newScoreMove;
        newScoreMove.score = -1000;
        newScoreMove.move = -1;

        //Los turnos estaban al reves 0(._.)0
        Turno _turno = depth % 2 == 0 ? Turno.cruz : Turno.circulo;

        // La victoria basta con comprobarlo solo para el turno anterior
        if (CondicionVictoria(_turno == Turno.circulo ? Turno.cruz : Turno.circulo, board)) newScoreMove.score = -100;
        else if (Empate(board)) newScoreMove.score = 0;
        else {
            if (depth != max_depth)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (board[i] == Turno.vacio)
                    {
                        board[i] = _turno; // hacemos un nuevo tablero para guardarlo y darselo a los siguientes nodos.
                        int valor_actual = -negamax(board, depth + 1).score;
                        board[i] = Turno.vacio;

                        Debug.Log(depth);
                        if (valor_actual > newScoreMove.score)// Aqui actualizo el valor y la casilla/mejor accion
                        {
                            newScoreMove.score = valor_actual;
                            newScoreMove.move = i;
                        }
                    }
                }
            }
            else newScoreMove.score = depth % 2 == 0 ? -otherEvaluation(_turno, board) : otherEvaluation(_turno, board);                    
        }   

        return newScoreMove; // devuelve el score y la mejor accion
    }
    #endregion negamaxFunc

    #region negamaxPrun
    ScoreMov negamaxAB(Turno[] board, int depth, int alpha, int beta)
    {
        ScoreMov newScoreMove;
        newScoreMove.score = -1000;
        newScoreMove.move = -1;

        Turno _turno = depth % 2 == 0 ? Turno.cruz : Turno.circulo;

        // La victoria basta con comprobarlo solo para el turno anterior
        if (CondicionVictoria(_turno == Turno.circulo ? Turno.cruz : Turno.circulo, board)) newScoreMove.score = -100;
        else if (Empate(board)) newScoreMove.score = 0;
        else
        {
            if (depth != max_depth)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (board[i] == Turno.vacio)
                    {
                        board[i] = _turno; 
                        int valor_actual = -negamaxAB(board, depth + 1, -beta, -Mathf.Max(alpha, newScoreMove.score)).score;
                        board[i] = Turno.vacio;

                        Debug.Log(depth);
                        if (valor_actual > newScoreMove.score)// Aqui actualizo el valor y la casilla/mejor accion
                        {
                            newScoreMove.score = valor_actual;
                            newScoreMove.move = i;
                        }
                        if(newScoreMove.score >= beta)
                        {
                            return newScoreMove;
                        }
                    }
                }
            }
            else newScoreMove.score = depth % 2 == 0 ? -otherEvaluation(_turno, board) : otherEvaluation(_turno, board);
        }

        return newScoreMove;
    }
    #endregion negamaxPrun

    #region busquedaAspitacional
    int previousScore = 0;  
    int windowRange = 20;   
    int minus_infinite = -10;   
    int infinite = 10;  
    ScoreMov AspirationSearch(Turno[] _board)
    {
        int alpha, beta;
        ScoreMov move;
        if(previousScore != 0)
        {
            alpha = previousScore - windowRange;
            beta = previousScore + windowRange;
            while (true)
            {
                move = negamaxAB(_board, 1, alpha, beta); // Cuando el negamaxAB funcione
                if (move.score <= alpha) alpha = minus_infinite;
                else if (move.score >= beta) beta = infinite;
                else break;
            }
            previousScore = move.score;
        }
        else
        {
            move = negamaxAB(_board, 1, minus_infinite, infinite);
            previousScore = move.score;
        }

        return move;
    }

    #endregion busquedaAspitacional

    #region negascout
    ScoreMov negascout(Turno[] _board, int depth, int alpha, int beta)
    {
        ScoreMov newScoreMove;
        newScoreMove.move = -1;
        newScoreMove.score = -1000;

        // Ruptura del bucle
        if (CondicionVictoria(Turno.circulo, board)) newScoreMove.score = +100;
        else if (CondicionVictoria(Turno.cruz, board)) newScoreMove.score = -100;
        else if (Empate(board)) newScoreMove.score = 0;
        if(depth == max_depth)
        {
            newScoreMove.score = otherEvaluation(Turno.circulo, _board);
        }
        else
        {
            // No tengo mejor movimiento, 
            int bestScore = newScoreMove.score;
            int bestMove = newScoreMove.move;

            //marco el valor de busqueda que estamos probando
            int adaptativeBeta = beta;

            //generamos los movimientos y los recorremos
            // llamamos a negamaxAB por cada movimiento. Le paso el alpha y beta adaptadas
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == Turno.vacio)
                {
                    newScoreMove = negamaxAB(board, depth + 1, -adaptativeBeta, -Mathf.Max(alpha, bestScore));
                    int currentScore = -newScoreMove.score;
                    int currentMove = newScoreMove.move;

                    //actualizamos el mejor score.
                    if (currentScore > bestScore)
                    {
                        if(adaptativeBeta == beta || depth >= max_depth - 2)
                        {
                            bestScore = currentScore;
                            bestMove = currentMove;
                        }
                        else
                        {
                            newScoreMove = negascout(board, depth, -beta, -currentScore);
                            bestScore = -newScoreMove.score;
                            bestMove = newScoreMove.move;
                        }
                    }

                    if (bestScore >= beta)
                    {
                        newScoreMove.score = bestScore;
                        newScoreMove.move = bestMove;
                        return newScoreMove;
                    }
                    // vamos ajustando alpha y beta
                    adaptativeBeta = Mathf.Max(alpha, bestScore) + 1;
                }
            }                   
        }

        return newScoreMove;
    }
    #endregion negascout

    private bool CondicionVictoria(Turno _turno, Turno[] _board)
    {
        bool victoria = false;

        // Sacamos nuestras condiciones de nuestro tablero, 8 combinaciones posibles:
        int[,] condiciones = new int[8, 3] { {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
                             {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, {0, 4, 8}, {2, 4, 6} };

        // Hacemos comprobación de las condiciones por si alguna se cumple.
        for(int i = 0; i < 8; i++)
        {
            if(_board[condiciones[i,0]] == _turno && _board[condiciones[i, 1]] == _turno && _board[condiciones[i, 2]] == _turno)
            {
                victoria = true;
            }
        }

        return victoria;
    }

    private int otherEvaluation(Turno _turno, Turno[] _board)
    {
        int evaluacion = 0;
        int count_line = 0;
        int count_empty = 0;
        int count_opponent = 0;

        int[,] condiciones = new int[8, 3] { {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
                             {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, {0, 4, 8}, {2, 4, 6} };
        for (int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                if(_board[condiciones[i,j]] == _turno){
                    count_line++;
                }
                else if(_board[condiciones[i, j]] == Turno.vacio)
                {
                    count_empty++;
                }
                else
                {
                    count_opponent++;
                }
                // Turno que le hemos pasado. Si tiene una ficha en una linea vacía +1, si dos fichas y un espacio vacío +10, y si tiene las 3 fichas +100
                if(count_line == 1 && count_empty == 2)
                {
                    evaluacion += 1;
                }
                else if(count_line == 2 && count_empty == 1)
                {
                    evaluacion += 10;
                }
                else if(count_line == 3)
                {
                    evaluacion += 100;
                }
                /// Turno rival. 
                else if (count_opponent == 1 && count_empty == 2)
                {
                    evaluacion -= 1;
                }
                else if (count_opponent == 2 && count_empty == 1)
                {
                    evaluacion -= 10;
                }
                else if (count_opponent == 3)
                {
                    evaluacion -= 100;
                }
            }
        }

        return evaluacion;
    }
    /*
    private int Evaluacion(Turno _turno, Turno[] _board)
    {
        int evaluacion = -500;
        int[,]condiciones = new int[24, 2] { {0, 1}, {1, 2}, {0, 2}, {3, 4}, {4, 5}, {3, 5}, {6, 7}, {7, 8}, {6, 8},{0, 3}, {3, 6},
                            {0, 6}, {1, 4}, {4, 7}, {1, 7}, {2, 5}, {5, 8}, {2, 8}, {0, 4}, {4, 8}, {0, 8}, {2, 4}, {4, 6}, {2, 6}  };

        for(int i = 0; i < 24; i++)
        {
            if (_board[condiciones[i, 0]] == _turno && _board[condiciones[i, 1]] == _turno)
            {
                evaluacion = +500;
            }
        }

        return evaluacion;
    }
    */
    private bool Empate(Turno[] _board)
    {
        bool vacio = false;
        for (int i = 0; i < 9; i++)
        {
            if (_board[i] == Turno.vacio)
            {
                vacio = true;
                break;
            }
        }

        // Si ni el jugador 1 ni el 2 han ganado y no hay espacios libres en el tablero, es un empate
        if (CondicionVictoria(Turno.cruz, _board) == false && CondicionVictoria(Turno.circulo, _board) == false && vacio == false)
            return true;
        else
            return false;
    }
}
