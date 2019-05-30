#pragma once
// operacje op(stos);
template <class T>
class operacje
{
public:
	void arg1(stos<T>* stos_ptr, double(*foo)(double d1));
	void arg2(stos<T>* stos_ptr, double(*foo)(double d1, double d2));

	//wieloargumentowe
	void med(stos<T>* stos_ptr);
	void avg(stos<T>* stos_ptr);
	void dev(stos<T>* stos_ptr);
	void var(stos<T>* stos_ptr);

};

template <class T>
void operacje<T>::med(stos<T>* stos_ptr)
{
	(*stos_ptr).sort();
	double med;
	element<T> *aux = (*stos_ptr).head;

	for (int i = 1; i < (*stos_ptr).size / 2; i++)
		aux = aux->next;

	if ((*stos_ptr).size % 2 == 0)
		med = (aux->value.num + aux->next->value.num) / 2;
	else
		med = aux->next->value.num;

	(*stos_ptr).clear();
	(*stos_ptr) += (med);
}

//poruwnuje znak operacji i wywoluje odpowiednia funkcje
//wzorzec polecenie
//lancuch zobowiazan
//gramatyka bacusa naura
//klasa parser
//lex i bizon



//wykonuje funckje foo na pierwszych 2 elementach ze sotsu i zostawia wynik na stosie
//definicje funckji + - * / itp. sa prawie identyczne dlatego przekazujemy wskaznik na funckje aby uniknac redundancji
template <class T>
void operacje<T>::arg2(stos<T>* stos_ptr, double(*foo)(double d1, double d2))
{
	double tmp = (*stos_ptr).pop();
	(*stos_ptr).set(foo((*stos_ptr).get(), tmp));
}
//wykonuje funkcje foo na pierwszym elemencie ze stosu
template <class T>
void operacje<T>::arg1(stos<T>* stos_ptr, double(*foo)(double d1))
{
	(*stos_ptr).set(foo((*stos_ptr).get()));
}

//srednia arytmetyczna
template <class T>
void operacje<T>::avg(stos<T>* stos_ptr)
{
	int i = 0;
	double sum = 0;
	while (!(*stos_ptr).is_empty())
	{
		i++;
		sum += stos_ptr->pop();
	}
	(*stos_ptr) += (sum / i);
}
//wariancja
template <class T>
void operacje<T>::var(stos<T>* stos_ptr)
{
	if ((*stos_ptr).is_empty())
		throw empty_stos();
	double var = 0, avg = 0;
	element<T> *aux = (*stos_ptr).head;
	while (aux)
	{
		var += aux->value.num * aux->value.num;
		avg += aux->value.num;
		aux = aux->next;
	}
	var = var / (*stos_ptr).size; //srednia kwadratow
	avg = avg / (*stos_ptr).size; //srednia

	var = (var - (avg * avg));
	(*stos_ptr).clear();
	(*stos_ptr) += var;
}
//odchylenie standardowe - pierwiastek z wariancji
template <class T>
void operacje<T>::dev(stos<T>* stos_ptr)
{
	var(stos_ptr);
	(*stos_ptr).set(std::sqrt((*stos_ptr).get()));
}
//znajduje mediane zadanego zbioru
