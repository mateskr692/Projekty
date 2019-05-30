#pragma once

//stores a list of elements explaining how to translate specified char into its Huffman bitcode
struct translation
{
	uint8_t ch;
	uint8_t bitsize;
	uint8_t *bittable;

	struct translation *next;
};

void translation_build_from_tree(struct translation **tablep, struct tree *root, uint8_t bitsize);
void translation_add(struct translation **tablep, struct tree *root, uint8_t bitsize);
struct translation *translation_find_char(struct translation *table, uint8_t ch);
void translation_delete(struct translation **tablep);
//debug only
void translation_print(struct translation *table);

