#pragma once
#define _PRECISION_ 4

//elemtent klasy number
//po numberze precion_number
//precyzja, notacja wykladnicza
//zastapic floata numberem
//operatory do numbera

class number
{
public:
	double num;

	number() {}
	number(double d) : num(d) {}

	operator double() { return num; }
	friend std::ostream& operator<<(std::ostream &os, number n)
	{
		return os << n.num;
	}
};

class precision_number : public number
{
public:
	int prec = _PRECISION_;
	friend std::ostream& operator<<(std::ostream &os, precision_number n)
	{
		return os << std::fixed << std::setprecision(n.prec) << n.num;
	}
};


class scientific_number : public number
{
public:
	friend std::ostream& operator<<(std::ostream &os, scientific_number n)
	{
		return os << std::scientific << n.num;
	}
};
