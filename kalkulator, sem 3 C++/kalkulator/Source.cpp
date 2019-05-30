#include "stdafx.h"
using namespace std;

/*
specyfikacja:
a)	uruchomienie programu z przelacznikamni -i -o spowoduje otwarcie pliku, obliczenie wartosci wyrazenia i zapisanie do pliku wyjsciowego
	kazda linijka w pliku wejsciowym to jedno dzialanie
	liczby i znaki sa odzielone znakami bialymi (spacja/tab, enter znaczy nowe dzialanie

b)	uruchomienie bez przelacznikow spowoduje wlaczenie trybu recznego
	rownania sa wczytywane z konsoli i wynik wypisywany do okna
	wczytywanie odbywa sie az do wpisania pustej wartosci (sam znak enter)

	np
	kalkulator -i input.txt -o output.txt
	
	kalkulator
	>>17 19 + 23 /
	<< 1.56...
	>>
	
operacje:
	dwuargumentowe: + - * / pow
	jednoargumentowe: ! sin cos log ln sqrt exp
	wieloargumentowe: med avg dev var

*/

void print_help()
{
	cout << "Sposob uzycia: kalkulator [-i xxx | -o xxx | -h]\n\n"
		"Bez argumentow\twczytuje ze standardowego wyjscia i wypisuje wynik na standardowe wyjscie\n"
		"-i xxx\t\twczytuje dane z pliku o sciezce xxx\n"
		"-o xxx\t\tzapisuje wyniik do pliku o sciezce xxx\n"
		"-h\t\twyswietla pomoc" << endl;
}

int main(int argc, char* argv[])
{

	//interpretacja argumentow 
	if (argc > 1 && strcmp(argv[1], "-h") == 0)
	{
		print_help();
		return 0;
	}
	string input_file, output_file;
	for (int i = 1; i < argc; i++)
	{
		if (strcmp(argv[i], "-i") == 0 && argc > i + 1)
		{
			input_file = argv[i + 1];
			i++;
		}
		else if (strcmp(argv[i], "-o") == 0 && argc > i + 1)
		{
			output_file = argv[i + 1];
			i++;
		}

		else
		{
			print_help();
			return 0;
		}
	}

	//stworzenie stosu i glownej strukury i wczystywanie i interpretacja danych
	logic<precision_number> B(input_file, output_file);
	try
	{
		B.read();
	}
	catch (unknown_operator)
	{
		cout << "nieznany operator" << endl;
		B.clear();
		return -1;
	}
	catch (empty_stos)
	{
		cout << "zbyt mala ilosc argumentow do wykonania operacji" << endl;
		B.clear();
		return -1;
	}
	catch(...) 
	{
		cout << "Nieznany blad" << endl;
		B.clear();
		return -1;
	}

	B.clear();
	return 1;

	//cin.ignore();
	//cin.get();
}