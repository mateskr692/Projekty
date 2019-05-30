#include "stdafx.h"

void list_add_char(struct list **headp, uint8_t ch)
//adds a tree with specified char into a list, if one already exists, increments its count
{
	struct list *aux = *headp;
	while (aux != NULL)
	{
		if (aux->subtree->ch == ch)
		{
			aux->subtree->count += 1;
			return;
		}
		aux = aux->next;
	}
	aux = (struct list*)malloc(sizeof(struct list));
	aux->subtree = (struct tree*)malloc(sizeof(struct tree));
	aux->subtree->ch = ch;
	aux->subtree->count = 1;
	aux->subtree->left = NULL;
	aux->subtree->right = NULL;
	aux->subtree->parent = NULL;

	aux->next = *headp;
	*headp = aux;
}
struct list *list_remove_lowest(struct list **headp)
//iterates through list and finds subtree with lowest counts, removes and returns list element containing that tree
{
	if (*headp == NULL)
		return *headp;
	//find lowest
	struct list *lowest = *headp;
	struct list *aux = *headp;
	while (aux != NULL)
	{
		if (aux->subtree->count < lowest->subtree->count)
			lowest = aux;
		aux = aux->next;
	}
	//remove lowest from list
	if (lowest == *headp)
	{
		*headp = lowest->next;
		lowest->next = NULL;
		return lowest;
	}
	aux = headp;
	while (aux->next != lowest)
		aux = aux->next;
	aux->next = lowest->next;
	lowest->next = NULL;
	return lowest;
}
void list_print(struct list *head)
//used for debugging to see if list got created properly.
{
	if (head == NULL)
	{
		printf("Empty list");
		return;
	}
	while (head != NULL)
	{
		printf("%c: %d\n", head->subtree->ch, head->subtree->count);
		head = head->next;
	}
}
