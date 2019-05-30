#pragma once
template <class T>
class element
{
public:
	T value;
	element* next;
	element(double x);
	element(double x, element *next);
};

template <class T>
element<T>::element(double x) : next(nullptr) { value.num = x; }

template <class T>
element<T>::element(double x, element<T> *next) : next(next) { value.num = x; }
