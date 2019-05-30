#include "stdafx.h"

memory::memory(): number(0) {}

memory& memory::operator+=(const double &num)
{
	number += num;
	return *this;
}

memory& memory::operator-=(const double &num)
{
	number -= num;
	return *this;
}

memory& memory::operator=(const double &num)
{
	number = num;
	return *this;
}

double memory::get()
{
	return number;
}