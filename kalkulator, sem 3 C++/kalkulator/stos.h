#pragma once
/*
	stos jednokierunkowy FILO
	dodawanie: nowy staje sie glowa i wskazuje na stara glowe
	usuwanie: stara glowa zostaje usunieta a nowa wskazuje na nastepny element po starej glowie
*/
template <class T>
class stos
{
	
public:
	element<T>* head;
	int size;

	stos<T>& operator+=(const double &num);
//	void add(double num);
	T pop();
	int is_empty();
	void clear();
	void sort();

	void set(double num);
	T get();
	stos();
	~stos();
};


template <class T>
stos<T>::stos() : head(nullptr), size(0) {}

//dodaje element o wartosci num na poczatek listy
template <class T>
stos<T>& stos<T>::operator+=(const double &num)
{
	element<T> *aux = new element<T>(num, head);
	head = aux;
	size += 1;

	return *this;
}
/*
void stos::add(double num)
{
	element *aux = new element(num, head);
	head = aux;
	size += 1;
}
*/

//usuwa pierwszy element listy i zwraca wartosc tego elementu
template <class T>
T stos<T>::pop()
{
	if (head == nullptr)
	{
		//throw empty_stos();
		T ret;
		ret.num = 0;
		return ret;
	}

	size -= 1;
	element<T> *aux = head;
	head = head->next;
	T d = aux->value;
	delete aux;
	return d;
}
//zmienia wartosc pierwszego elementu na stosie
//uzywane w celu unikniecia dealokacji pamieci aby zaraz ja zaalokowac ponownie
template <class T>
void stos<T>::set(double num)
{
	if (head == nullptr)
		throw empty_stos();

	head->value.num = num;
}
//zwraca wartosc pierwszego elementu na stosie
template <class T>
T stos<T>::get()
{
	if (head == nullptr)
		throw empty_stos();

	return head->value;
}

//zwraca true gdy lista jest pusta
template <class T>
int stos<T>::is_empty() { return (head == nullptr); }

//usuwa wszystkie elementy z listy
template <class T>
void stos<T>::clear()
{
	while (head != nullptr)
	{
		element<T> *aux = head;
		head = head->next;
		delete aux;
	}
	size = 0;
}

//czysci liste
template <class T>
stos<T>::~stos()
{
	clear();
}
//buuble sort, uzywane do wyznaczenia mediany zbioru
template <class T>
void stos<T>::sort()
{
	if (head == nullptr)
		throw empty_stos();

	for (int i = 0; i < size; i++)
	{
		element<T> *aux = head;
		for (int j = 0; j < size - i - 1; j++)
		{
			if (aux->value.num > aux->next->value.num)
			{
				T tmp = aux->value;
				aux->value = aux->next->value;
				aux->next->value = tmp;
			}
			aux = aux->next;
		}
	}
}
