#include "stdafx.h"
//huffman.exe -1 "tekst1.txt" -o "output"
//huffman.exe -o "tekst2.txt" -i "output.huffman" -d
//huffman.exe -h

int main(int argc, char **argv[])
{
	uint8_t mode, input_arg, output_arg; //mode = 0 -> encode, mode = 1 -> decode
	mode = input_arg = output_arg = 0;
	FILE *input, *output;
	
	//handle main arguments
	if (argc > 1 && strcmp(argv[1], "-h") == 0)
	{
		printf("Usage: huffman -e|-d, -i <input>, -o <output>\n\n-e: encodes file\n-d: decodes file\n-i <input>: specifies input file\n-o <output>: specifies output file\nIf -e -d arguments are ommited program encodes given file\nArguments are parsed in any order\n");
		return 1;
	}
	//arguments can be parsed in any order and if -e | -d is ommited, program works in encode mode by default
	for (int i = 1; i < argc; i++)
	{
		if (strcmp(argv[i], "-e") == 0)
		{
			mode = 0;
		}
		else if (strcmp(argv[i], "-d") == 0)
		{
			mode = 1;
		}
		else if (strcmp(argv[i], "-i") == 0 && argc > i + 1)
		{
			input_arg = i + 1;
			i++;
		}
		else if (strcmp(argv[i], "-o") == 0 && argc > i + 1)
		{
			output_arg = i + 1;
			i++;
		}
		else
		{
			printf("Error: Invaalid arguments, use -h for more information\n");
			return 2;
		}
	}
	//validate input and output files (needs to validate output since its possible that system wont be able to create an output file with specified name)
	if (input_arg == 0 || output_arg == 0)
	{
		printf("Error: Missing external files\n");
		return 2;
	}
	//only allow to decode files with .huffman extension
	if (mode == 1)
	{
		char *st1 = argv[input_arg];
		char *st2 = malloc(9);
		strcpy(st2, ".huffman");
		for (int i = 1; i < 9; i++)
		{
			if (strlen(st1) < 8 || st1[strlen(st1) - i] != st2[strlen(st2) - i])
			{
				printf("Error: Invalid input extension\n");
				free(st2);
				return 2;
			}
		}
		free(st2);
	}

	input = fopen(argv[input_arg], "rb");
	if (input == NULL)
	{
		printf("Error: Unable to access input file\n");
		return 2;
	}
	//add .huffman extension to encoded file
	
	if (mode == 0)
	{
		char* outp = malloc(strlen(argv[output_arg]) + 9);
		strcpy(outp, argv[output_arg]);
		strcat(outp, ".huffman");
		output = fopen(outp, "wb");
		free(outp);
	}
	else
		output = fopen(argv[output_arg], "wb");

	if (output == NULL)
	{
		printf("Error: Unable to access output file\n");
		fclose(input);
		return 2;
	}

	//perform the encoding/decoding
	if (mode == 0)
		demo_encode(input, output);
	else if (mode == 1)
		demo_decode(input, output);

	return 0;
}
