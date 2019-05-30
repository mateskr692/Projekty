#pragma once
class memory
{
	double number;
public:
	memory();
	memory& operator+=(const double &num);
	memory& operator-=(const double &num);
	memory& operator=(const double &num);
	
	double get();
};

