﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    public enum algoritmo { MINIMAX, NEGAMAX, NEGAMAXPRUN, ASPIRATION_SEARCH, NEGASCOUT, MTD_SSS, MTD_f };
    public algoritmo alg;

    // El score es el resultado que nos darán los algoritmos como valor. El movimiento será la casilla correspondiente a ese valor.
    public struct ScoreMov
    {
        public int score;
        public int move;

        public ScoreMov(int x, int y)
        {
            score = x;
            move = y;
        }
    }
    ScoreMov scoreMov;

    [SerializeField] int max_depth = 50;


    //////////////////////////////////////////////////////////////
    /// Para el MTD
    protected ZobristKey31Bits zobristKeys;
    protected Transposition_table transpositionTable;
    public int hashTableLength = 90000000;
    private int maximumExploredDepth = 0;
    private int globalGuess = 1000;
    public int MAX_ITERATIONS = 10;

    /////////////////////
    public static Stopwatch m_stopwatch = new Stopwatch();

    void Awake()
    {
        turno = Turno.cruz;
        turno_texto.text = "Turno del Jugador 1";

        // Desde el principio van a estar vacíos y luego los iremos rellenando según vaya avanzando el juego.
        for (int i = 0; i < 9; i++)
            board[i] = Turno.vacio;

        ////////////////////////////////////
        /// Para el MTD y Test
        zobristKeys = GetComponent<ZobristKey31Bits>();

        zobristKeys = new ZobristKey31Bits(42, 2);
        zobristKeys.Print();
        transpositionTable = new Transposition_table(hashTableLength);
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
        ////////////////////////////////////////////////////////////////
        ///Algoritmos
        
        #region minimax
        if (turno == Turno.circulo && alg == algoritmo.MINIMAX)
        {
            m_stopwatch.Reset();
            m_stopwatch.Start();

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

            m_stopwatch.Stop();
            UnityEngine.Debug.Log("Minimax: " + m_stopwatch.ElapsedMilliseconds + " milisegundos");

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (mejor_casilla > -1)
            {
                casillas[mejor_casilla].SetActive(false);
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
        if (turno == Turno.circulo && alg == algoritmo.NEGAMAX) 
        {
            m_stopwatch.Reset();
            m_stopwatch.Start();

            scoreMov = negamax(board, 1);

            m_stopwatch.Stop();
            UnityEngine.Debug.Log("Negamax: " + m_stopwatch.ElapsedMilliseconds + " milisegundos");

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos].SetActive(false);
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
            m_stopwatch.Reset();
            m_stopwatch.Start();

            scoreMov = negamaxAB(board, 1, -1000, 1000);

            m_stopwatch.Stop();
            UnityEngine.Debug.Log("NegamaxAB: " + m_stopwatch.ElapsedMilliseconds + " milisegundos");

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos].SetActive(false);
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
            m_stopwatch.Reset();
            m_stopwatch.Start();

            scoreMov = AspirationSearch(board);

            m_stopwatch.Stop();
            UnityEngine.Debug.Log("Aspiration Search: " + m_stopwatch.ElapsedMilliseconds + " milisegundos");

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos].SetActive(false);
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
            m_stopwatch.Reset();
            m_stopwatch.Start();

            scoreMov = negascout(board, 0, -1000, 1000);

            m_stopwatch.Stop();
            UnityEngine.Debug.Log("Negascout: " + m_stopwatch.ElapsedMilliseconds + " milisegundos");

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos].SetActive(false);
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

        #region MTD_SSS
        if (turno == Turno.circulo && alg == algoritmo.MTD_SSS)
        {
            m_stopwatch.Reset();
            m_stopwatch.Start();

            scoreMov = MTD(board, globalGuess);

            m_stopwatch.Stop();
            UnityEngine.Debug.Log("MTD-SSS: " + m_stopwatch.ElapsedMilliseconds + " milisegundos");

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos].SetActive(false);
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
        #endregion MTD_SSS

        #region MTD_f
        if (turno == Turno.circulo && alg == algoritmo.MTD_f)
        {
            m_stopwatch.Reset();
            m_stopwatch.Start();

            scoreMov = MTD(board, 10);

            m_stopwatch.Stop();
            UnityEngine.Debug.Log("MTD-f: " + m_stopwatch.ElapsedMilliseconds + " milisegundos");

            int bestScore = scoreMov.score;
            int bestPos = scoreMov.move;

            // Si hemos elegido una casilla entonces ponemos la ficha que corresponde y nos guardamos de quien fue el turno para saber cual es la ficha.
            if (bestPos > -1)
            {
                casillas[bestPos].SetActive(false);
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
        #endregion MTD_f

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
            return evaluate(_turno);
        }
        int valor;

        if(_turno == Turno.circulo)
        {
            // Vamos comprobando qué resultado lógico saldría de cada opción que tenemos en el tablero (las casillas vacías).
            for(int i = 0; i < 9; i++)
            {
                if(_board[i] == Turno.vacio)
                {
                    UnityEngine.Debug.Log("nodo");
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

        //Si es par es turno del jugador, si no es de la IA
        Turno _turno = depth % 2 == 0 ? Turno.cruz : Turno.circulo;

        // La victoria basta con comprobarla solo para el turno anterior
        if (CondicionVictoria(_turno == Turno.circulo ? Turno.cruz : Turno.circulo, board)) newScoreMove.score = -100;
        else if (Empate(board)) newScoreMove.score = 0;
        // Si no hemos llegado a un estado final comprobamos si al menos hemos llegado a la profundidad máxima
        else {
            if (depth != max_depth)
            {
                for (int i = 0; i < 9; i++) // Recorremos todos los posibles movimientos
                {
                    if (board[i] == Turno.vacio)
                    {
                        UnityEngine.Debug.Log("nodo");
                        board[i] = _turno; // hacemos un nuevo tablero para guardarlo y darselo a los siguientes nodos.
                        int valor_actual = -negamax(board, depth + 1).score;
                        board[i] = Turno.vacio;

                        if (valor_actual > newScoreMove.score)// Aqui actualizo el valor y la casilla/mejor accion
                        {
                            newScoreMove.score = valor_actual;
                            newScoreMove.move = i;
                        }
                    }
                }
            }
            else newScoreMove.score = depth % 2 == 0 ? -evaluate(_turno) : evaluate(_turno);                    
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
                        UnityEngine.Debug.Log("nodo");
                        board[i] = _turno; 
                        int valor_actual = -negamaxAB(board, depth + 1, -beta, -Mathf.Max(alpha, newScoreMove.score)).score;
                        board[i] = Turno.vacio;

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
            else newScoreMove.score = depth % 2 == 0 ? -evaluate(_turno) : evaluate(_turno);
        }

        return newScoreMove;
    }
    #endregion negamaxPrun

    #region busquedaAspitacional
    int previousScore = 0;  
    int windowRange = 20;   
    int minus_infinite = -100;   
    int infinite = 100;  
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
            newScoreMove.score = evaluate(Turno.circulo);
        }
        else
        {
            // No tengo mejor movimiento, 
            int bestScore = newScoreMove.score;
            int bestMove = newScoreMove.move;

            // marco el valor de busqueda que estamos probando
            int adaptativeBeta = beta;

            // generamos los movimientos y los recorremos
            // llamamos a negamaxAB por cada movimiento. Le paso el alpha y beta adaptadas
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == Turno.vacio)
                {
                    newScoreMove = negamaxAB(board, depth + 1, -adaptativeBeta, -Mathf.Max(alpha, bestScore));
                    int currentScore = -newScoreMove.score;
                    int currentMove = newScoreMove.move;

                    // actualizamos el mejor score.
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

    #region Test
    int get_hash_value()
    {
        int hc = board.Length;
        for (int i = 0; i < board.Length; ++i) hc = unchecked(hc * 314159 + (int)board[i]);
        return hc;
    }

    public class Transposition_table
    {
        public int length;
        Dictionary<int, Board_record> records = new Dictionary<int, Board_record>();
        public Transposition_table(int _length) => length = _length;

        public void save_record(Board_record record) => records[record.hash_value % length] = record;

        public Board_record GetRecord(int hash)
        {
            int key = hash % length;
            if (records.ContainsKey(key)) return records[key].hash_value == hash ? records[key] : null;
            return null;
        }
    }

    Transposition_table transposition_table = new Transposition_table(99999);


    public class Board_record
    {
        public int hash_value, min_score, max_score, best_move, depth;
        public Board_record(int _hash_value = 0, int _min_score = 0, int _max_score = 0, int _best_move = 0, int _depth = 0)
        {
            hash_value = _hash_value;
            min_score = _min_score;
            max_score = _max_score;
            best_move = _best_move;
            depth = _depth;
        }
    }

    //Comprueba si es la ultima rama de recursion: condicion de victoria, empate o maxima profundidad alcanzada
    bool should_end(int depth, bool is_player_turn) => CondicionVictoria(is_player_turn ? Turno.circulo : Turno.cruz, board) || Empate(board) || depth == max_depth;

    int max_explored_depth = 0;

    ScoreMov Test(Turno[] board, int depth, int gamma)
    {
        bool is_player_turn = (depth & 1) != 1; // True si es turno del jugador
        ScoreMov scoreMove = new ScoreMov(minus_infinite, -1);

        if (depth > max_explored_depth) max_explored_depth = depth; // Se actualiza la maxima profundidad alcanzada. 

        Board_record record = transposition_table.GetRecord(get_hash_value()); // Busca si este tablero ya existe en la memoria

        if (record != null) // Si tenemos un registro de este tablero.
        {
            // Se comprueba si la profundidad es adecuada (si la profundidad explorada de la tabla sacada de la memoria es mayor que la que exploramos ahora. 
            // porque si el valor que contiene no es acertado habra que seguir explorando mas profundidad por si se encuentra una jugada mejor)
            if (record.depth > max_depth - depth) // Si el score se ajusta al valor gamma que arrastramos, entonces devolvemos la jugada adecuada.
                if (record.min_score > gamma) return new ScoreMov(record.min_score, record.best_move);
                else if (record.max_score < gamma) return new ScoreMov(record.max_score, record.best_move);
        }
        // Si no hay un registro de este tablero, lo creamos para guardarlo despues en memoria
        else record = new Board_record(get_hash_value(), max_depth - depth, minus_infinite, 1000);

        //A partir de aqui es como un negamax 

        // Si estamos en la última rama de la recursión se evalua la puntuación
        if (should_end(depth, is_player_turn))
            scoreMove = new ScoreMov(record.min_score = record.max_score = evaluate(is_player_turn ? Turno.cruz : Turno.circulo), -1);
        else // Si no, búsca la siguiente jugada
            for (int move = 0; move != 9; ++move)
            { // Se explora todas las jugadas posibles
                if (board[move] == Turno.vacio)
                {
                    UnityEngine.Debug.Log("nodo");
                    // Se explora las ramas con backtracking para no crear copias del tablero
                    board[move] = is_player_turn ? Turno.cruz : Turno.circulo;
                    int score = -Test(board, (byte)(depth + 1), -gamma).score;
                    board[move] = Turno.vacio;

                    if (score > scoreMove.score) // Se actualiza el mejor score
                    {
                        record.best_move = move;
                        scoreMove.score = score;
                        scoreMove.move = move;
                    }
                    // Min score y max score creo que funcionan como el alpha y beta
                    if (scoreMove.score < gamma) record.max_score = scoreMove.score;
                    else record.min_score = scoreMove.score;
                }
            }
        transposition_table.save_record(record); // Se guarda el valor por si se vuelve a obtener la misma jugada en otra rama
        return scoreMove;
    }
    #endregion Test

    #region MTD
    public ScoreMov MTD(Turno[] board, int globalGuess)
    {
        int i;
        int gamma, guess = globalGuess;
        ScoreMov scoringMove = new ScoreMov(0, -1);
        maximumExploredDepth = 0;

        string output = "";
        for (i = 0; i < MAX_ITERATIONS; i++)
        {
            gamma = guess;
            scoringMove = Test(board, 0, gamma - 1);
            guess = scoringMove.score;
            if (gamma == guess)
            {
                globalGuess = guess;
                output += "guess encontrado en iteracion " + i;
                return scoringMove;
            }
        }
        output += "guess no encontrado";
        globalGuess = guess;
        return scoringMove;
    }
    #endregion MTD

    #region Condiciones_Evaluacion
    bool CondicionVictoria(Turno _turno, Turno[] _board)
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
    int evaluate(Turno _turno)
    {
        int evaluacion = 0;
        int count_line = 0;
        int count_empty = 0;
        int count_opponent = 0;

        int[,] condiciones = new int[8, 3] { {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
                            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, {0, 4, 8}, {2, 4, 6} };
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 3; j++)
            { 
                if (board[condiciones[i, j]] == _turno) count_line++;
                else if (board[condiciones[i, j]] == Turno.vacio) count_empty++;
                else count_opponent++;
            }
            
            // Turno que le hemos pasado. Si tiene una ficha en una linea vacía +1, si dos fichas y un espacio vacío +10, y si tiene las 3 fichas +100
            if (count_line == 1 && count_empty == 2) evaluacion += 1;
            else if (count_line == 2 && count_empty == 1) evaluacion += 10;
            else if (count_line == 3) evaluacion += 100;

            // Turno rival. 
            else if (count_opponent == 1 && count_empty == 2) evaluacion -= 1;
            else if (count_opponent == 2 && count_empty == 1) evaluacion -= 10;
            else if (count_opponent == 3) evaluacion -= 100;

            //Reiniciamos los contadores
            count_line = count_empty = count_opponent = 0;
        }
        return evaluacion;
    }

    bool Empate(Turno[] _board)
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
    #endregion Condiciones_Evaluacion
}
