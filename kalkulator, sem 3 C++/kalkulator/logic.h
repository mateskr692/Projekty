#pragma once
#include "stdafx.h"
using namespace std;

template <class T>
class logic
{
	stos<T> S;
	operacje<T> op;
	memory mem;
	std::ifstream input;
	std::ofstream output;
public:
	logic(const std::string &inp, const std::string &out);

	void read();
	void process(const std::string &line);
	void calculate(const std::string &str);
	void clear();
};

//sprawdzanie poprawnosci strumieni do pliku odbywa sie przy odczytywaniu, zapisaniu
//gdy strumienie plikowe sa nie wlasciwe odbedzie sie wczytywanie i zapis z konsoli
template <class T>
logic<T>::logic(const std::string &inp, const std::string &out) :S(), mem()
{
	input.open(inp);
	output.open(out);
}
template <class T>
void logic<T>::read()
{
	string line;
	while (true)
	{
		//pobranie lini ze strumienia wejsciowego
		if (!input.is_open())
			getline(cin, line);
		else
			getline(input, line);
		if (line.compare("") == 0 || input.eof())
			break;

		process(line);
		//przymij ostatni elemnt na stosie jako wynik operacji
		T w = S.pop();

		//zapis wyniku na wyjscie
		if (!output.is_open())
			cout << w << endl;
		else
			output << w << endl;

		S.clear();
	}
}
//dzieli linie (dzialanie) na poszczegolne liczby i operatory i dodaje do stosu / wykonuje operacje
template <class T>
void logic<T>::process(const std::string &line)
{
	std::istringstream ss(line);
	string arg;
	while (ss >> arg)
	{
		//jezeli nie da sie zamienic na double, uznaj za operator, else, dodaj na stos
		try
		{
			double d = std::stod(arg);
			S += d;
		}
		catch (invalid_argument)
		{
			//wyrzucenie wyjatku w tym miejscu spowoduje powrot "na gore", wyszyczenie zalokowanej pamieci i zakonczenie programu
			calculate(arg);
		}
	}
}

template <class T>
void logic<T>::calculate(const std::string &str)
{
	if (str.compare("+") == 0)								//dodawanie
		op.arg2(&S, [](double a, double b) {return a + b; });
	else if (str.compare("-") == 0)							//odejmowanie
		op.arg2(&S, [](double a, double b) {return a - b; });
	else if (str.compare("*") == 0)							//mnozenie
		op.arg2(&S, [](double a, double b) {return a * b; });
	else if (str.compare("/") == 0)							//dzielenie
		op.arg2(&S, [](double a, double b) {return a / b; });
	else if (str.compare("^") == 0)							//potega
		op.arg2(&S, std::pow);

	else if (str.compare("!") == 0)							//liczba przeciwna
		op.arg1(&S, [](double a) {return -a; });
	else if (str.compare("over") == 0)						//liczba odwrotna
		op.arg1(&S, [](double a) {return 1 / a; });
	else if (str.compare("sin") == 0)						//sinus
		op.arg1(&S, std::sin);
	else if (str.compare("cos") == 0)						//cosinus
		op.arg1(&S, std::cos);
	else if (str.compare("log10") == 0)						//logarytm o podstawie 10
		op.arg1(&S, std::log10);
	else if (str.compare("log") == 0)						//logarytm naturalny
		op.arg1(&S, std::log);
	else if (str.compare("sqrt") == 0)						//pierwiastek kwadratowy
		op.arg1(&S, std::sqrt);
	else if (str.compare("exp") == 0)						//funckja wykladnicza
		op.arg1(&S, std::exp);

	else if (str.compare("avg") == 0)						//srednia arytmetyczna
		op.avg(&S);
	else if (str.compare("med") == 0)						//mediana
		op.med(&S);
	else if (str.compare("dev") == 0)						//odchylenie standardowe z proby
		op.dev(&S);
	else if (str.compare("var") == 0)						//wariancja
		op.var(&S);

	else if (str.compare("M+") == 0)						//dodaj do pamieci
		mem += S.get();
	else if (str.compare("M-") == 0)						//odejmij od pamieci
		mem -= S.get();
	else if (str.compare("MS") == 0)						//zapisz do pamieci
		mem = S.get();
	else if (str.compare("MC") == 0)						//wyczysc pamiec
		mem = 0;
	else if (str.compare("MR") == 0)						//dodaj zawartosc pamieci na stos
		S += mem.get();

	else if (str.compare("C") == 0)							//wyczysc stos
		S.clear();

	else
		throw unknown_operator();
}

//zamyka otwarte strumienie i usuwa stos
template <class T>
void logic<T>::clear()
{
	input.close();
	output.close();
	S.clear();
}
