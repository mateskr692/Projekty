#pragma once

struct buffer
{
	uint8_t byte;
	int8_t free_bits;
	FILE *f;
};

struct buffer* buffer_initiate(FILE *filename);
void buffer_send(struct buffer *b, uint8_t byte, int8_t bitsize);
uint8_t buffer_load_1bit(struct buffer *b);
uint8_t buffer_load_8bit(struct buffer *b);

