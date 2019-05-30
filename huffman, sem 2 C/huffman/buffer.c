#include "stdafx.h"

struct buffer* buffer_initiate(FILE *filename)
{
	struct buffer *b = (struct buffer*)malloc(sizeof(struct buffer));
	b->byte = 0;
	b->free_bits = 8;
	b->f = filename;

	return b;
}
void buffer_send(struct buffer *b, uint8_t byte, int8_t bitsize)
{
	while (bitsize > 0)
	{
		uint8_t bs = bitsize;
		bitsize -= b->free_bits;
		b->byte |= (byte >> (8 - b->free_bits));
		byte <<= b->free_bits;
		b->free_bits -= bs;
		if (b->free_bits <= 0)
		{
			putc(b->byte, b->f);
			b->free_bits = 8;
			b->byte = 0;
		}
	}
}
uint8_t buffer_load_1bit(struct buffer *b)
{
	if (b->free_bits == 8)
	{
		b->byte = getc(b->f);
		b->free_bits = 0;
	}
	uint8_t bit = (b->byte >> 7); //0000 0000 || 0000 0001
	b->byte <<= 1;
	b->free_bits += 1;

	return bit;
}
uint8_t buffer_load_8bit(struct buffer *b)
{
	uint8_t byte = b->byte;
	b->byte = getc(b->f);
	byte |= (b->byte >> (8 - b->free_bits));
	b->byte <<= b->free_bits;

	return byte;
}

