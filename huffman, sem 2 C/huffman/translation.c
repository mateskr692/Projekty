#include "stdafx.h"

void translation_build_from_tree(struct translation **tablep, struct tree *root, uint8_t bitsize)
{
	if (root->left != NULL)
	{
		translation_build_from_tree(tablep, root->left, bitsize + 1);
		translation_build_from_tree(tablep, root->right, bitsize + 1);
		return;
	}
	else
	{
		translation_add(tablep, root, bitsize);
	}

}
void translation_add(struct translation **tablep, struct tree *root, uint8_t bitsize)
//figures the characteres' Huffman code and stores it in the entry
{
	struct translation *entry = (struct translation*)malloc(sizeof(struct translation));
	entry->bitsize = bitsize;
	entry->ch = root->ch;
	entry->next = *tablep;
	*tablep = entry;
	//create a table storing no more bits than nessecary (1-8 bits = 1 table element, 9-16 = 2 etc)
	uint8_t tabsize = (bitsize - 1) / 8 + 1;
	entry->bittable = (uint8_t*)malloc(tabsize * sizeof(uint8_t));
	for (int i = 0; i < tabsize; i++)
		entry->bittable[i] = 0; //00000000
	//follow the parent root and create bitoce for char
	uint8_t bitposition = bitsize;
	while (root->parent != NULL)
	{
		//change bit to 1 at proper positions
		if (root->parent->right == root)
		{
			uint8_t bp = bitposition;
			uint8_t tabindex = 0;
			while (bp > 8)
			{
				tabindex += 1;
				bp -= 8;
			}
			entry->bittable[tabindex] |= (1 << (8 - bp));
		}
		bitposition -= 1;
		root = root->parent;
	}
	return;
}
struct translation *translation_find_char(struct translation *table, uint8_t ch)
{
	while (table->ch != ch)
		table = table->next;

	return table;
}
void translation_delete(struct translation **tablep)
{
	while (*tablep != NULL)
	{
		struct translation *aux = *tablep;
		*tablep = aux->next;
		free(aux->bittable);
		free(aux);
	}
}

void translation_print(struct translation *table)
{
	while (table != NULL)
	{
		printf("%d: ", table->ch);
		for (int i = 0; i < (table->bitsize - 1) / 8 + 1; i++)
		{
			uint8_t n = table->bittable[i];
			for (int i = 7; i >= 0; i--)
			{
				if (n & (1 << i))
					printf("1");
				else
					printf("0");
			}
			printf(" ");
		}

		printf("(%d bits)\n", table->bitsize);
		table = table->next;
	}
}